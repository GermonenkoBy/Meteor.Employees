using Meteor.Employees.Core.Contracts;
using Meteor.Employees.Core.Extensions;
using Meteor.Employees.Core.Models.Enums;
using Meteor.Employees.Infrastructure.Services.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Meteor.Employees.Infrastructure.Services;

public class MigrationsRunner : IMigrationsRunner
{
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

    public async Task RunMigrationsAsync(CancellationToken cancellationToken = new())
    {
        using var scope = _serviceProvider.CreateScope();
        var customersClient = scope.ServiceProvider.GetRequiredService<ICustomersClient>();
        var customerMigrationsRunner = scope.ServiceProvider.GetRequiredService<ISingleCustomerMigrationsRunner>();

        var customersCount = await customersClient.GetCustomersCountAsync(_nonTerminatedCustomerStatuses);
        for (int customerIndex = 0; customerIndex < customersCount; customerIndex++)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            var response = await customersClient.GetCustomersPageAsync(new()
            {
                Statuses = _nonTerminatedCustomerStatuses,
                Paging = new()
                {
                    Limit = 1,
                    Offset = customerIndex
                }
            });
            var customer = response.Items.FirstOrDefault();
            if (customer is null)
            {
                continue;
            }

            await customerMigrationsRunner.RunForCustomer(customer, cancellationToken);
        }
    }
}