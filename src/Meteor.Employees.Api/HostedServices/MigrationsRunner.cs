using Meteor.Common.Core.Models;
using Meteor.Employees.Core;
using Meteor.Employees.Core.Contracts;
using Meteor.Employees.Core.Extensions;
using Meteor.Employees.Core.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement;

namespace Meteor.Employees.Api.HostedServices;

public class MigrationsRunner : BackgroundService
{
    private const int CustomersBatchSize = 100;

    private readonly IServiceProvider _serviceProvider;

    private readonly List<CustomerStatuses> _nonTerminatedCustomerStatuses = new()
    {
        CustomerStatuses.New,
        CustomerStatuses.Active,
        CustomerStatuses.Suspended,
    };

    public MigrationsRunner(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RunMigrationAsync(stoppingToken);
    }

    private async Task RunMigrationAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var featureManager = scope.ServiceProvider.GetRequiredService<IFeatureManager>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<MigrationsRunner>>();

        var runMigrations = await featureManager.IsEnabledAsync("RunMigrationsOnStartup");
        if (!runMigrations)
        {
            return;
        }

        try
        {
            await RunMigrationsAsync(scope, stoppingToken);
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "Failed to run migrations.");
        }
    }

    private async Task RunMigrationsAsync(IServiceScope scope, CancellationToken stoppingToken)
    {
        var customersClient = scope.ServiceProvider.GetRequiredService<ICustomersClient>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<MigrationsRunner>>();

        var customersCount = await customersClient.GetCustomersCountAsync(_nonTerminatedCustomerStatuses);
        for (var offset = 0; offset < customersCount; offset += CustomersBatchSize)
        {
            var paging = new Paging
            {
                Offset = offset,
                Limit = CustomersBatchSize
            };

            var customersPage = await customersClient.GetCustomersPageAsync(new()
            {
                Statuses = _nonTerminatedCustomerStatuses,
                Paging = paging,
            });

            foreach (var customer in customersPage.Items)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    logger.LogWarning("Service is stopping while migration is in progress.");
                    return;
                }

                logger.LogInformation($"Running migration for {customer.Name} ({customer.Domain}).");

                var settings = await customersClient.GetCustomerSettingsAsync(customer.Id);
                using var customerSubScope = _serviceProvider.CreateScope();
                var customerDataAccessor = customerSubScope.ServiceProvider.GetRequiredService<ICustomerDataAccessor>();
                customerDataAccessor.Settings = settings;

                await using var context = customerSubScope.ServiceProvider.GetRequiredService<EmployeesContext>();
                await context.Database.MigrateAsync(cancellationToken: stoppingToken);

                logger.LogInformation($"Successfully completed migration for {customer.Name} ({customer.Domain}).");
            }
        }
    }
}