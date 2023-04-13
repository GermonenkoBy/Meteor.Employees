using Grpc.Core;
using Mapster;
using MapsterMapper;
using Meteor.Common.Core.Exceptions;
using Meteor.Employees.Api.HostedServices;
using Meteor.Employees.Api.Services;
using Meteor.Employees.Api.Services.Contracts;
using Meteor.Employees.Core;
using Meteor.Employees.Core.Contracts;
using Meteor.Employees.Infrastructure.Contracts;
using Meteor.Employees.Infrastructure.Grpc;
using Meteor.Employees.Infrastructure.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.FeatureManagement;

var builder = WebApplication.CreateBuilder(args);

var appConfigurationConnectionString = builder.Configuration.GetConnectionString("AzureAppConfiguration");
if (!string.IsNullOrEmpty(appConfigurationConnectionString))
{
    builder.Configuration.AddAzureAppConfiguration(options =>
    {
        options
            .Connect(appConfigurationConnectionString)
            .UseFeatureFlags()
            .Select(KeyFilter.Any)
            .Select(KeyFilter.Any, builder.Environment.EnvironmentName)
            .Select(KeyFilter.Any, $"{builder.Environment.EnvironmentName}-Employees");
    });
}

builder.Services.AddFeatureManagement();

builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

builder.Services.AddHostedService<MigrationsRunner>();
builder.Services.AddScoped<ICustomersClient, GrpcCustomersClient>();
builder.Services.AddScoped<ICustomerDataAccessor, CustomerDataAccessor>();
builder.Services.AddSingleton<ICustomerIdProvider, HeadersCustomerIdProvider>();

var config = new TypeAdapterConfig();
config.Apply(new InfrastructureMappingsRegister());
builder.Services.AddSingleton<IMapper>(new Mapper(config));

builder.Services.AddGrpcClient<CustomersService.CustomersServiceClient>(options =>
{
    var url = builder.Configuration.GetValue<string>("Routing:ControllerUrl") ?? string.Empty;
    options.Address = new Uri(url);
    options.ChannelOptionsActions.Add(opt => opt.Credentials = ChannelCredentials.Insecure);
});

builder.Services.AddDbContext<EmployeesContext>(
    (services, options) =>
    {
        var customerDataAccessor = services.GetRequiredService<ICustomerDataAccessor>();
        if (customerDataAccessor.Settings is null)
        {
            throw new MeteorException("Unable to connect to customer database. Customer settings are not set.");
        }

        options
            .UseNpgsql(
                customerDataAccessor.Settings.CoreConnectionString,
                opt => opt.MigrationsAssembly("Meteor.Employees.Migrations")
            )
            .UseSnakeCaseNamingConvention()
            .EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
    }
);

var app = builder.Build();

app.MapGrpcReflectionService();
app.Run();