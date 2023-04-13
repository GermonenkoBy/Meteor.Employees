using Grpc.Core;
using MapsterMapper;
using Meteor.Common.Core.Models;
using Meteor.Employees.Core.Contracts;
using Meteor.Employees.Core.Dtos;
using Meteor.Employees.Infrastructure.Grpc;

namespace Meteor.Employees.Infrastructure.Contracts;

public class GrpcCustomersClient : ICustomersClient
{
    private readonly CustomersService.CustomersServiceClient _grpcClient;

    private readonly IMapper _mapper;

    public GrpcCustomersClient(CustomersService.CustomersServiceClient grpcClient, IMapper mapper)
    {
        _grpcClient = grpcClient;
        _mapper = mapper;
    }

    public async Task<Core.Models.CustomerSettings?> GetCustomerSettingsAsync(int customerId)
    {
        try
        {
            var response = await _grpcClient.GetCustomerSettingsAsync(new()
            {
                CustomerId = customerId,
            });

            return new()
            {
                CoreConnectionString = response.CoreConnectionString,
            };
        }
        catch (RpcException e) when(e.Status.StatusCode == StatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<PagedResult<Core.Models.Customer>> GetCustomersPageAsync(CustomersFilter filter)
    {
        var request = _mapper.Map<GetCustomersPageRequest>(filter);
        var response = await _grpcClient.GetCustomersAsync(request);
        var customers = _mapper.Map<List<Core.Models.Customer>>(response.Customers);
        return new (customers, response.Total);
    }
}