using Mapster;
using Meteor.Employees.Core.Dtos;
using Meteor.Employees.Infrastructure.Grpc;

namespace Meteor.Employees.Infrastructure.Mapping;

public class InfrastructureMappingsRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.ForType<CustomersFilter, GetCustomersPageRequest>()
            .Map(req => req.Limit, filter => filter.Paging.Limit)
            .Map(req => req.Offset, filter => filter.Paging.Offset);
    }
}