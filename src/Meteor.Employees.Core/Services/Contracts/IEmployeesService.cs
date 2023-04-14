using Meteor.Employees.Core.Dtos;

using Meteor.Employees.Core.Models;

namespace Meteor.Employees.Core.Services.Contracts;

public interface IEmployeesService
{
    Task<Employee> GetEmployeeAsync(int employeeId);

    Task<Employee> GetEmployeeAsync(string emailAddress);

    Task<Employee> CreateEmployeeAsync(CreateEmployeeDto employeeDto);
}