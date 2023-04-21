using Meteor.Employees.Api.Enums;
using Meteor.Employees.Api.HealthChecks;
using Meteor.Employees.Infrastructure.Services.Contracts;
using Microsoft.FeatureManagement;

namespace Meteor.Employees.Api.Jobs;

public class MigrationsJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public MigrationsJob(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
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
            var logger = _serviceProvider.GetRequiredService<ILogger<MigrationsJob>>();
            migrationsHealthCheck.MigrationsStatus = MigrationsStatuses.Error;
            logger.LogCritical(e, "Filed to apply migrations");
        }
    }
}