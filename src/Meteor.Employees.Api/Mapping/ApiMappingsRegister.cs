using Mapster;
using Meteor.Common.Core.Exceptions;
using Meteor.Employees.Api.Grpc;

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
    }
}