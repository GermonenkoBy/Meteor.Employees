using Meteor.Employees.Core.Models;

namespace Meteor.Employees.Infrastructure.Services.Contracts;

public interface ISingleCustomerMigrationsRunner
{
    Task RunForCustomer(Customer customer, CancellationToken cancellationToken = new());
}