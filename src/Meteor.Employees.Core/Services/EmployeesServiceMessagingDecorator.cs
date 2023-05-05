using MapsterMapper;
using Meteor.Common.Messaging.Abstractions;
using Meteor.Employees.Core.Contracts;
using Meteor.Employees.Core.Dtos;
using Meteor.Employees.Core.Models;
using Meteor.Employees.Core.Services.Contracts;

namespace Meteor.Employees.Core.Services;

public class EmployeesServiceMessagingDecorator : IEmployeesService
{
    private readonly IEmployeesService _service;

    private readonly IMapper _mapper;

    private readonly IPublisher<EmployeeChangedMessage> _updateEmployeePublisher;

    private readonly IPublisher<EmployeeRemovedMessage> _removeEmployeePublisher;

    private readonly ICustomerDataAccessor _customerDataAccessor;

    public EmployeesServiceMessagingDecorator(
        IEmployeesService service,
        IMapper mapper,
        IPublisher<EmployeeChangedMessage> updateEmployeePublisher,
        IPublisher<EmployeeRemovedMessage> removeEmployeePublisher,
        ICustomerDataAccessor customerDataAccessor
    )
    {
        _service = service;
        _mapper = mapper;
        _updateEmployeePublisher = updateEmployeePublisher;
        _removeEmployeePublisher = removeEmployeePublisher;
        _customerDataAccessor = customerDataAccessor;
    }

    public Task<Employee> GetEmployeeAsync(int employeeId)
    {
        return _service.GetEmployeeAsync(employeeId);
    }

    public Task<Employee> GetEmployeeAsync(string emailAddress)
    {
        return _service.GetEmployeeAsync(emailAddress);
    }

    public async Task<Employee> CreateEmployeeAsync(CreateEmployeeDto employeeDto)
    {
        var employee = await _service.CreateEmployeeAsync(employeeDto);
        var message = _mapper.Map<EmployeeChangedMessage>(employee);
        message.CustomerId = _customerDataAccessor.CustomerId;
        await _updateEmployeePublisher.PublishAsync(message);
        return employee;
    }

    public async Task<Employee> UpdateEmployeeAsync(int employeeId, UpdateEmployeeDto employeeDto)
    {
        var employee = await _service.UpdateEmployeeAsync(employeeId, employeeDto);
        var message = _mapper.Map<EmployeeChangedMessage>(employee);
        message.CustomerId = _customerDataAccessor.CustomerId;
        await _updateEmployeePublisher.PublishAsync(message);
        return employee;
    }

    public async Task RemoveEmployeeAsync(int employeeId)
    {
        await _service.RemoveEmployeeAsync(employeeId);
        await _removeEmployeePublisher.PublishAsync(new()
        {
            EmployeeId = employeeId,
            CustomerId = _customerDataAccessor.CustomerId
        });
    }

    public Task<bool> ValidatePassword(int employeeId, string password)
    {
        return _service.ValidatePassword(employeeId, password);
    }
}
