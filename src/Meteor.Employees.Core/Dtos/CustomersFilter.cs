using Meteor.Common.Core.Models;
using Meteor.Employees.Core.Models.Enums;

namespace Meteor.Employees.Core.Dtos;

public record struct CustomersFilter()
{
    public List<CustomerStatuses> Statuses = new();

    public Paging Paging = new();
}