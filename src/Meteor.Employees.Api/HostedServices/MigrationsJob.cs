using Meteor.Employees.Infrastructure.Services.Contracts;

namespace Meteor.Employees.Api.HostedServices;

public class MigrationsJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public MigrationsJob(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var migrationsRunner = _serviceProvider.GetRequiredService<IMigrationsRunner>();

        try
        {
            await migrationsRunner.RunMigrationsAsync(stoppingToken);
        }
        catch (Exception e)
        {
            var logger = _serviceProvider.GetRequiredService<ILogger<MigrationsJob>>();
            logger.LogCritical(e, "Filed to apply migrations");
        }
    }
}