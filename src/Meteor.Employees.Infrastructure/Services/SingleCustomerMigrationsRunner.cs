using Meteor.Employees.Core;
using Meteor.Employees.Core.Contracts;
using Meteor.Employees.Core.Models;
using Meteor.Employees.Infrastructure.Services.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Meteor.Employees.Infrastructure.Services;

public class SingleCustomerMigrationsRunner : ISingleCustomerMigrationsRunner
{
    private readonly IServiceProvider _serviceProvider;

    public SingleCustomerMigrationsRunner(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task RunForCustomer(Customer customer, CancellationToken cancellationToken = new())
    {
        using var scope = _serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<SingleCustomerMigrationsRunner>>();
        var customersClient = scope.ServiceProvider.GetRequiredService<ICustomersClient>();
        var customerDataAccessor = scope.ServiceProvider.GetRequiredService<ICustomerDataAccessor>();

        logger.LogInformation(
            "Applying migrations for {CustomerName} ({CustomerDomain})",
            customer.Name,
            customer.Domain
        );

        var customerSettings = await customersClient.GetCustomerSettingsAsync(customer.Id);
        if (customerSettings is null)
        {
            logger.LogError(
                "Failed to apply migrations for {CustomerName} ({CustomerDomain}): customer settings are not setup",
                customer.Name,
                customer.Domain
            );
        }

        customerDataAccessor.Settings = customerSettings;

        try
        {
            await using var employeesContext = scope.ServiceProvider.GetRequiredService<EmployeesContext>();
            await employeesContext.Database.MigrateAsync(cancellationToken);
            logger.LogInformation(
                "Successfully applied migrations for {CustomerName} ({CustomerDomain})",
                customer.Name,
                customer.Domain
            );
        }
        catch (Exception e)
        {
            logger.LogCritical(
                e,
                "Failed to apply migrations for {CustomerName} ({CustomerDomain})",
                customer.Name,
                customer.Domain
            );
        }
    }
}
