namespace Meteor.Employees.Api.Services.Contracts;

public interface ICustomerIdProvider
{
    public bool TryGetCustomerId(HttpContext httpContext, out int customerId);
}