using Mapster;
using Meteor.Employees.Core.Models;

namespace Meteor.Employees.Core.Dtos;

using Models.Enums;

public record struct EmployeeChangedMessage
{
    [AdaptMember(nameof(Employee.Id))]
    public int EmployeeId;

    public int CustomerId;

    public string FirstName;

    public string LastName;

    public string EmailAddress;

    public string PhoneNumber;

    public EmployeeStatuses Status;
}
