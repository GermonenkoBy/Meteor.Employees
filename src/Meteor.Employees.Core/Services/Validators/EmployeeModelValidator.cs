using System.ComponentModel.DataAnnotations;
using Meteor.Employees.Core.Models;
using Meteor.Employees.Core.Services.Validators.Contracts;

namespace Meteor.Employees.Core.Services.Validators;

public class EmployeeModelValidator : IValidator<Employee>
{
    public Task<bool> TryValidateAsync(Employee employee, ICollection<ValidationResult> validationResults)
    {
        var valid = Validator.TryValidateObject(
            employee,
            new ValidationContext(employee),
            validationResults,
            true
        );
        return Task.FromResult(valid);
    }
}