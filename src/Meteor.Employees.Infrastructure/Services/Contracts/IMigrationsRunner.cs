namespace Meteor.Employees.Infrastructure.Services.Contracts;

public interface IMigrationsRunner
{
    Task RunMigrationsAsync(CancellationToken cancellationToken = new());
}