﻿using Grpc.Core;
using MapsterMapper;
using Meteor.Common.Core.Exceptions;
using Meteor.Employees.Api.Grpc;
using Meteor.Employees.Core.Dtos;
using Meteor.Employees.Core.Services.Contracts;

namespace Meteor.Employees.Api.Services;

public class EmployeesGrpcService : EmployeesService.EmployeesServiceBase
{
    private readonly IEmployeesService _employeesService;

    private readonly IMapper _mapper;

    public EmployeesGrpcService(IEmployeesService employeesService, IMapper mapper)
    {
        _employeesService = employeesService;
        _mapper = mapper;
    }

    public override async Task<Employee> GetEmployeeById(GetEmployeeByIdRequest request, ServerCallContext context)
    {
        var employee = await _employeesService.GetEmployeeAsync(request.Id);
        return _mapper.Map<Employee>(employee);
    }

    public override async Task<Employee> GetEmployeeByEmail(GetEmployeeByEmailRequest request, ServerCallContext context)
    {
        var employee = await _employeesService.GetEmployeeAsync(request.EmailAddress);
        return _mapper.Map<Employee>(employee);
    }

    public override async Task<SaveEmployeeResponse> CreateEmployee(
        CreateEmployeeRequest request,
        ServerCallContext context
    )
    {
        var dto = _mapper.Map<CreateEmployeeDto>(request);

        try
        {
            var employee = await _employeesService.CreateEmployeeAsync(dto);
            return new()
            {
                Employee = _mapper.Map<Employee>(employee)
            };
        }
        catch (MeteorValidationException e)
        {
            return new()
            {
                ValidationResult = _mapper.Map<ValidationResult>(e)
            };
        }
    }
}