using System.ComponentModel.DataAnnotations;
using Meteor.Common.Core.Exceptions;
using Meteor.Employees.Core.Helpers;
using Meteor.Employees.Core.Models;
using Meteor.Employees.Core.Services.Validators.Contracts;
using Moq;

namespace Meteor.Employees.Core.Tests.Helpers;

[TestClass]
public class ValidatorsRunnerTests
{
    private readonly Mock<IValidator<Employee>> _firstValidator = new();

    private readonly Mock<IValidator<Employee>> _secondValidator = new();

    private readonly IEnumerable<IValidator<Employee>> _validators;

    public ValidatorsRunnerTests()
    {
        _validators = new[]
        {
            _firstValidator.Object,
            _secondValidator.Object,
        };
    }

    [TestMethod]
    public async Task RunValidatorsAgainstValidModel_ShouldNot_ThrowException()
    {
        _firstValidator
            .Setup(v => v.TryValidateAsync(It.IsAny<Employee>(), It.IsAny<ICollection<ValidationResult>>()))
            .ReturnsAsync(true);

        _secondValidator
            .Setup(v => v.TryValidateAsync(It.IsAny<Employee>(), It.IsAny<ICollection<ValidationResult>>()))
            .ReturnsAsync(true);

        await ValidatorsRunner.EnsureIsValid(new Employee(), _validators);

        _firstValidator.VerifyAll();
        _secondValidator.VerifyAll();
    }

    [TestMethod]
    public async Task RunValidatorsAgainstInvalid_Should_RunUntilItEncountersFirstErrorAndThrowException()
    {
        _firstValidator
            .Setup(v => v.TryValidateAsync(It.IsAny<Employee>(), It.IsAny<ICollection<ValidationResult>>()))
            .ReturnsAsync(false);

        _secondValidator
            .Setup(v => v.TryValidateAsync(It.IsAny<Employee>(), It.IsAny<ICollection<ValidationResult>>()))
            .ReturnsAsync(true);

        await Assert.ThrowsExceptionAsync<MeteorValidationException>(async () =>
        {
            await ValidatorsRunner.EnsureIsValid(new Employee(), _validators, "", false);
        });

        _firstValidator.VerifyAll();
        _secondValidator.Verify(
            v => v.TryValidateAsync(It.IsAny<Employee>(), It.IsAny<ICollection<ValidationResult>>()),
            Times.Never
        );
    }

    [TestMethod]
    public async Task RunValidatorsAgainstInvalid_Should_ThrowException()
    {
        _firstValidator
            .Setup(v => v.TryValidateAsync(It.IsAny<Employee>(), It.IsAny<ICollection<ValidationResult>>()))
            .ReturnsAsync(false);

        _secondValidator
            .Setup(v => v.TryValidateAsync(It.IsAny<Employee>(), It.IsAny<ICollection<ValidationResult>>()))
            .ReturnsAsync(false);

        await Assert.ThrowsExceptionAsync<MeteorValidationException>(async () =>
        {
            await ValidatorsRunner.EnsureIsValid(new Employee(), _validators);
        });

        _firstValidator.VerifyAll();
        _secondValidator.VerifyAll();
    }
}