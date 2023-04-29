using Meteor.Common.Core.Models;
using Meteor.Employees.Core.Dtos;
using Meteor.Employees.Core.Models;

namespace Meteor.Employees.Core.Contracts;

public interface ICustomersClient
{
    Task<Customer?> GetCustomerAsync(int customerId);

    Task<CustomerSettings?> GetCustomerSettingsAsync(int customerId);

    Task<PagedResult<Customer>> GetCustomersPageAsync(CustomersFilter filter);
}