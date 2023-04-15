namespace Meteor.Employees.Core.Dtos;

public record struct CreateEmployeeDto
{
    public string EmailAddress;

    public string PhoneNumber;

    public string FirstName;

    public string LastName;

    public string Password;

    public bool Activate;
}