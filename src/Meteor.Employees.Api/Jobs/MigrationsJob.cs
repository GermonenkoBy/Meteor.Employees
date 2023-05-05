using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Meteor.Employees.Api.Dtos;
using Meteor.Employees.Api.Enums;
using Meteor.Employees.Api.HealthChecks;
using Meteor.Employees.Core.Contracts;
using Meteor.Employees.Infrastructure.Services.Contracts;
using Microsoft.FeatureManagement;

namespace Meteor.Employees.Api.Jobs;

public class MigrationsJob : IHostedService, IAsyncDisposable
{
    private const string MigrationsTopicName = "migrations";

    private const string SubscriptionName = "employees-service";

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        IncludeFields = true,
        PropertyNameCaseInsensitive = true,
    };

    private readonly IServiceProvider _serviceProvider;

    private readonly ServiceBusClient _serviceBusClient;

    private readonly ServiceBusProcessor _serviceBusProcessor;

    private readonly ILogger<MigrationsJob> _logger;

    public MigrationsJob(IServiceProvider serviceProvider, ILogger<MigrationsJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _serviceBusClient = serviceProvider.GetRequiredService<ServiceBusClient>();
        _serviceBusProcessor = _serviceBusClient.CreateProcessor(MigrationsTopicName, SubscriptionName);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await StartProcessingAsync(cancellationToken);
        await RunStartupMigrationsAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await StopProcessingAsync(cancellationToken);
    }

    private async Task RunStartupMigrationsAsync(CancellationToken stoppingToken)
    {
        var featureManager = _serviceProvider.GetRequiredService<IFeatureManager>();
        var migrationsHealthCheck = _serviceProvider.GetRequiredService<MigrationsHealthCheck>();
        if (!await featureManager.IsEnabledAsync("RunMigrationsOnStartup"))
        {
            migrationsHealthCheck.MigrationsStatus = MigrationsStatuses.Disabled;
            return;
        }

        var migrationsRunner = _serviceProvider.GetRequiredService<IMigrationsRunner>();

        migrationsHealthCheck.MigrationsStatus = MigrationsStatuses.InProgress;
        try
        {
            await migrationsRunner.RunMigrationsAsync(stoppingToken);
            migrationsHealthCheck.MigrationsStatus = MigrationsStatuses.Completed;
        }
        catch (Exception e)
        {
            migrationsHealthCheck.MigrationsStatus = MigrationsStatuses.Error;
            _logger.LogCritical(e, "Filed to apply migrations");
        }
    }

    private async Task StartProcessingAsync(CancellationToken cancellationToken)
    {
        _serviceBusProcessor.ProcessMessageAsync += ProcessMessageAsync;
        _serviceBusProcessor.ProcessErrorAsync += ProcessErrorAsync;
        await _serviceBusProcessor.StartProcessingAsync(cancellationToken);
    }

    private async Task StopProcessingAsync(CancellationToken cancellationToken)
    {
        await _serviceBusProcessor.StopProcessingAsync(cancellationToken);
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs arg)
    {
        _logger.LogError(arg.Exception, "Unhandled exception throw by the service bus processor;");
        return Task.CompletedTask;
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs arg)
    {
        try
        {
            var customers = arg.Message.Body.ToObjectFromJson<MigrationsTriggerDto>(_jsonSerializerOptions);
            foreach (var customerId in customers.CustomerIds)
            {
                await ApplyCustomerMigrations(customerId);
            }

            await arg.CompleteMessageAsync(arg.Message);
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Failed apply migrations. Message received: {Message}", arg.Message.Body.ToString());
            await arg.DeadLetterMessageAsync(arg.Message);
        }
    }

    private async Task ApplyCustomerMigrations(int customerId)
    {
        using var scope = _serviceProvider.CreateScope();
        var customersClient = scope.ServiceProvider.GetRequiredService<ICustomersClient>();
        var customerDataAccessor = scope.ServiceProvider.GetRequiredService<ICustomerDataAccessor>();

        var customer = await customersClient.GetCustomerAsync(customerId);
        if (customer is null)
        {
            _logger.LogWarning("Unable to run migrations: customer with ID {CustomerId} was not found", customerId);
            return;
        }

        var customerSettings = await customersClient.GetCustomerSettingsAsync(customerId);
        if (customerSettings is null)
        {
            _logger.LogWarning(
                "Unable to run migrations for customer {Customer} ({Domain}): settings are not set up",
                customer.Name,
                customer.Domain
            );
            return;
        }

        customerDataAccessor.Settings = customerSettings;
        var migrationsRunner = scope.ServiceProvider.GetRequiredService<ISingleCustomerMigrationsRunner>();
        await migrationsRunner.RunForCustomer(customer);
    }

    public async ValueTask DisposeAsync()
    {
        await _serviceBusProcessor.DisposeAsync();
        await _serviceBusClient.DisposeAsync();
    }
}