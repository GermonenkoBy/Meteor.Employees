using Mapster;
using Meteor.Employees.Core.Dtos;
using Meteor.Employees.Core.Models;
using Meteor.Employees.Core.Models.Enums;

namespace Meteor.Employees.Core.Mapping;

public class CoreMappingsRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.ForType<CreateEmployeeDto, Employee>()
            .Map(e => e.Status, dto => dto.Activate ? EmployeeStatuses.Active : EmployeeStatuses.Inactive)
            .Ignore(e => e.Id)
            .Ignore(e => e.PasswordHash)
            .Ignore(e => e.PasswordSalt);

        config.ForType<UpdateEmployeeDto, Employee>()
            .Ignore(e => e.PasswordHash)
            .Ignore(e => e.PasswordSalt)
            .IgnoreNullValues(true);
    }
}