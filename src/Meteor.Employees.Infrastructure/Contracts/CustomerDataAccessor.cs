using Meteor.Employees.Core.Contracts;
using Meteor.Employees.Core.Models;

namespace Meteor.Employees.Infrastructure.Contracts;

public class CustomerDataAccessor : ICustomerDataAccessor
{
    public CustomerSettings? Settings { get; set; }
}