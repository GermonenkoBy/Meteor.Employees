namespace Meteor.Employees.Core.Dtos;

using Models.Enums;

public record struct EmployeeChangedMessage
{
    public int Id;

    public string FirstName;

    public string LastName;

    public string EmailAddress;

    public string PhoneNumber;

    public EmployeeStatuses Status;
}
