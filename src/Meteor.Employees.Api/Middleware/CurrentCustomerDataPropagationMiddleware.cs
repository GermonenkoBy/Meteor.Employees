using Meteor.Employees.Api.Services.Contracts;
using Meteor.Employees.Core.Contracts;

namespace Meteor.Employees.Api.Middleware;

public class CurrentCustomerDataPropagationMiddleware : IMiddleware
{
    private readonly ICustomersClient _customersClient;

    private readonly IEnumerable<ICustomerIdProvider> _customerIdProviders;

    private readonly ICustomerDataAccessor _customerDataAccessor;

    public CurrentCustomerDataPropagationMiddleware(
        ICustomersClient customersClient,
        IEnumerable<ICustomerIdProvider> customerIdProviders,
        ICustomerDataAccessor customerDataAccessor
    )
    {
        _customersClient = customersClient;
        _customerIdProviders = customerIdProviders;
        _customerDataAccessor = customerDataAccessor;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        await TrySetCustomerData(context);
        await next.Invoke(context);
    }

    private async Task TrySetCustomerData(HttpContext context)
    {
        int customerId = default;
        foreach (var customerIdProvider in _customerIdProviders)
        {
            if (customerIdProvider.TryGetCustomerId(context, out customerId))
            {
                break;
            }
        }

        if (customerId == default)
        {
            return;
        }

        _customerDataAccessor.CustomerId = customerId;

        var customerSettings = await _customersClient.GetCustomerSettingsAsync(customerId);
        if (customerSettings is not null)
        {
            _customerDataAccessor.Settings = customerSettings;
        }
    }
}