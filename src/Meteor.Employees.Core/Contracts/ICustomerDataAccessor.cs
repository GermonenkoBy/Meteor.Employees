using Meteor.Employees.Core.Models;

namespace Meteor.Employees.Core.Contracts;

public interface ICustomerDataAccessor
{
    CustomerSettings? Settings { get; set; }
}