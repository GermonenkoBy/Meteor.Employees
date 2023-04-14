using System.ComponentModel.DataAnnotations;
using Meteor.Employees.Core.Models;
using Meteor.Employees.Core.Services.Validators.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Meteor.Employees.Core.Services.Validators;

public class EmployeeUniqueConstraintsValidator : IValidator<Employee>
{
    private readonly EmployeesContext _context;

    public EmployeeUniqueConstraintsValidator(EmployeesContext context)
    {
        _context = context;
    }

    public async Task<bool> TryValidateAsync(Employee employee, ICollection<ValidationResult> validationResults)
    {
        var duplicateEmployees = await _context.Employees
            .AsNoTracking()
            .Where(e => e.Id != employee.Id
                        && (e.EmailAddress == employee.EmailAddress || e.PhoneNumber == employee.PhoneNumber)
            )
            .ToListAsync();

        if (duplicateEmployees.Count == 0)
        {
            return true;
        }

        if (duplicateEmployees.Any(e => e.EmailAddress == employee.EmailAddress))
        {
            var result = new ValidationResult("Email address is in use", new[] { nameof(Employee.EmailAddress) });
            validationResults.Add(result);
        }

        if (duplicateEmployees.Any(e => e.PhoneNumber == employee.PhoneNumber))
        {
            var result = new ValidationResult("Phone number is in use", new[] { nameof(Employee.PhoneNumber) });
            validationResults.Add(result);
        }

        return false;
    }
}