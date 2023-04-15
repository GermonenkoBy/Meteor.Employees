using System.ComponentModel.DataAnnotations;
using Meteor.Employees.Core.Models;
using Meteor.Employees.Core.Services.Validators;
using Microsoft.EntityFrameworkCore;

namespace Meteor.Employees.Core.Tests.Services.Validators;

[TestClass]
public class EmployeeUniqueConstraintsValidatorTests
{
    private static readonly EmployeesContext Context;

    private static readonly EmployeeUniqueConstraintsValidator Validator;

    private const string DuplicateEmailAddress = "duplicate@email.address";

    private const string DuplicatePhoneNumber = "1234567890";

    static EmployeeUniqueConstraintsValidatorTests()
    {
        var contextOptions = new DbContextOptionsBuilder<EmployeesContext>()
            .UseInMemoryDatabase(nameof(EmployeeUniqueConstraintsValidatorTests))
            .Options;

        Context = new(contextOptions);

        Validator = new(Context);
    }

    [ClassInitialize]
    public static void Initialize(TestContext testContext)
    {
        Context.Employees.Add(new Employee
        {
            Id = 1,
            EmailAddress = DuplicateEmailAddress,
            PhoneNumber = DuplicatePhoneNumber,
            FirstName = "FirstName",
            LastName = "LastName"
        });

        Context.SaveChanges();
        Context.ChangeTracker.Clear();
    }

    [TestMethod]
    public async Task RunValidatorAgainstDuplicateEmployee_Should_ReturnFalseAndAddErrors()
    {
        var employee = new Employee
        {
            EmailAddress = DuplicateEmailAddress,
            PhoneNumber = DuplicatePhoneNumber,
        };

        var errors = new List<ValidationResult>();
        var valid = await Validator.TryValidateAsync(employee, errors);
        Assert.IsFalse(valid);
        Assert.AreEqual(2, errors.Count);
    }

    [TestMethod]
    public async Task RunValidatorAgainstValidEmployee_Should_ReturnTrue()
    {
        var employee = new Employee
        {
            EmailAddress = $"{DuplicateEmailAddress}2",
            PhoneNumber = $"{DuplicatePhoneNumber}2",
        };

        var errors = new List<ValidationResult>();
        var valid = await Validator.TryValidateAsync(employee, errors);
        Assert.IsTrue(valid);
        Assert.IsFalse(errors.Any());
    }
}