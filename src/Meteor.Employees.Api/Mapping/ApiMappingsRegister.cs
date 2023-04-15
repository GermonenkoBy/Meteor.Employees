using Mapster;
using Meteor.Common.Core.Exceptions;
using Meteor.Employees.Api.Grpc;
using Meteor.Employees.Core.Dtos;

namespace Meteor.Employees.Api.Mapping;

public class ApiMappingsRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.ForType<MeteorValidationException, ValidationResult>()
            .Map(vr => vr.Message, e => e.Message)
            .AfterMapping((ex, vr) =>
            {
                var errors = ex.Errors.Select(er => new ValidationError
                {
                    Message = er.ErrorMessage ?? "[Unknown Error]",
                    Fields = { er.MemberNames }
                });
                vr.Errors.AddRange(errors);
            });

        config.ForType<UpdateEmployeeRequest, UpdateEmployeeDto>()
            .Map(dto => dto.FirstName, req => req.FirstName, req => req.HasFirstName)
            .Map(dto => dto.LastName, req => req.LastName, req => req.HasLastName)
            .Map(dto => dto.EmailAddress, req => req.EmailAddress, req => req.HasEmailAddress)
            .Map(dto => dto.PhoneNumber, req => req.PhoneNumber, req => req.HasPhoneNumber)
            .Map(dto => dto.Password, req => req.Password, req => req.HasPassword)
            .Map(dto => dto.Status, req => req.Status, req => req.HasStatus);
    }
}