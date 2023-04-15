using Meteor.Employees.Core.Models.Enums;

namespace Meteor.Employees.Core.Dtos;

public record struct UpdateEmployeeDto
{
    public string? EmailAddress;

    public string? PhoneNumber;

    public string? FirstName;

    public string? LastName;

    public string? Password;

    public EmployeeStatuses? Status;
}