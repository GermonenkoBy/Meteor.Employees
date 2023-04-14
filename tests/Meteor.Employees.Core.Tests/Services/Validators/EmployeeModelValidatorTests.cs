using System.ComponentModel.DataAnnotations;
using Meteor.Employees.Core.Models;
using Meteor.Employees.Core.Services.Validators;

namespace Meteor.Employees.Core.Tests.Services.Validators;

[TestClass]
public class EmployeeModelValidatorTests
{
    private readonly EmployeeModelValidator _validator = new();

    [TestMethod]
    public async Task RunValidatorAgainstValidModel_Should_ReturnTrue()
    {
        var employee = new Employee
        {
            FirstName = "First name",
            LastName = "Last name",
            EmailAddress = "email@address.com",
            PhoneNumber = "1234567890",
            PasswordSalt = Array.Empty<byte>(),
            PasswordHash = Array.Empty<byte>()
        };

        var validationResults = new List<ValidationResult>();
        var valid = await _validator.TryValidateAsync(employee, validationResults);
        Assert.IsTrue(valid);
        Assert.IsFalse(validationResults.Any());
    }

    [TestMethod]
    public async Task RunValidatorAgainstInvalidModel_Should_ReturnFalseAndAddValidationResults()
    {
        var employee = new Employee
        {
            FirstName = string.Empty,
            LastName = string.Empty,
            EmailAddress = "email@address.com",
            PhoneNumber = "1234567890",
            PasswordSalt = Array.Empty<byte>(),
            PasswordHash = Array.Empty<byte>()
        };

        var validationResults = new List<ValidationResult>();
        var valid = await _validator.TryValidateAsync(employee, validationResults);
        Assert.IsFalse(valid);
        Assert.AreEqual(2, validationResults.Count);
    }
}