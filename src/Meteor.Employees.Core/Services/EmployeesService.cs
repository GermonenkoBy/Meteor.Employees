using MapsterMapper;
using Meteor.Common.Core.Exceptions;
using Meteor.Common.Cryptography.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Meteor.Employees.Core.Services;

using Contracts;
using Validators.Contracts;
using Dtos;
using Helpers;
using Models;

public class EmployeesService : IEmployeesService
{
    private const string EmployeeDataErrorMessage = "Employee data is invalid.";

    private readonly EmployeesContext _context;

    private readonly IMapper _mapper;

    private readonly IHasher _hasher;

    private readonly IEnumerable<IValidator<Employee>> _validators;

    public EmployeesService(
        EmployeesContext context,
        IMapper mapper,
        IHasher hasher,
        IEnumerable<IValidator<Employee>> validators
    )
    {
        _context = context;
        _mapper = mapper;
        _hasher = hasher;
        _validators = validators;
    }

    public async Task<Employee> GetEmployeeAsync(int employeeId)
    {
        var employee = await _context.Employees.FindAsync(employeeId);
        if (employee is null)
        {
            throw new MeteorNotFoundException("Employee not found.");
        }

        return employee;
    }

    public async Task<Employee> GetEmployeeAsync(string emailAddress)
    {
        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.EmailAddress == emailAddress);
        if (employee is null)
        {
            throw new MeteorNotFoundException("Employee not found.");
        }

        return employee;
    }

    public async Task<Employee> CreateEmployeeAsync(CreateEmployeeDto employeeDto)
    {
        var employee = _mapper.Map<Employee>(employeeDto);
        (employee.PasswordHash, employee.PasswordSalt) = _hasher.Hash(employeeDto.Password);

        await ValidatorsRunner.EnsureIsValid(employee, _validators, EmployeeDataErrorMessage);

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        return employee;
    }

    public async Task<Employee> UpdateEmployeeAsync(int employeeId, UpdateEmployeeDto employeeDto)
    {
        var employee = await _context.Employees.FindAsync(employeeId);
        if (employee is null)
        {
            throw new MeteorNotFoundException("Employee not found.");
        }

        _mapper.Map(employeeDto, employee);
        if (employeeDto.Password is not null)
        {
            (employee.PasswordHash, employee.PasswordSalt) = _hasher.Hash(employeeDto.Password);
        }

        await ValidatorsRunner.EnsureIsValid(employee, _validators, EmployeeDataErrorMessage);

        await _context.SaveChangesAsync();
        return employee;
    }

    public async Task<bool> ValidatePassword(int employeeId, string password)
    {
        var employee = await _context.Employees.FindAsync(employeeId);
        if (employee is null)
        {
            throw new MeteorNotFoundException("Employee not found.");
        }

        var hashToCompare = _hasher.Hash(password, employee.PasswordSalt);
        return hashToCompare.SequenceEqual(employee.PasswordHash);
    }

    public async Task RemoveEmployeeAsync(int employeeId)
    {
        var employee = await _context.Employees.FindAsync(employeeId);
        if (employee is null)
        {
            return;
        }

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();
    }
}