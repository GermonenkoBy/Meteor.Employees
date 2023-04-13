using Meteor.Common.Core.Models;
using Meteor.Employees.Core.Contracts;
using Meteor.Employees.Core.Models.Enums;

namespace Meteor.Employees.Core.Extensions;

public static class CustomersClientExtensions
{
    private static Paging EmptyPage => new()
    {
        Limit = 0,
        Offset = 0,
    };

    public static async Task<int> GetCustomersCountAsync(this ICustomersClient client, List<CustomerStatuses> statuses)
    {
        var customersPage = await client.GetCustomersPageAsync(new()
        {
            Statuses = statuses,
            Paging = EmptyPage,
        });

        return customersPage.Total;
    }
}