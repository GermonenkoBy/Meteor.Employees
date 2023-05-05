namespace Meteor.Employees.Core.Dtos;

public record EmployeeRemovedMessage
{
    public int EmployeeId;

    public int CustomerId;
}
