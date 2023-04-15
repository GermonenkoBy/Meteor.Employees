using Grpc.Core;
using Mapster;
using MapsterMapper;
using Meteor.Common.Cryptography.DependencyInjection.Extensions;
using Meteor.Employees.Api.HostedServices;
using Meteor.Employees.Api.Interceptors;
using Meteor.Employees.Api.Mapping;
using Meteor.Employees.Api.Middleware;
using Meteor.Employees.Api.Services;
using Meteor.Employees.Api.Services.Contracts;
using Meteor.Employees.Core;
using Meteor.Employees.Core.Contracts;
using Meteor.Employees.Core.Exceptions;
using Meteor.Employees.Core.Mapping;
using Meteor.Employees.Core.Models;
using Meteor.Employees.Core.Services;
using Meteor.Employees.Core.Services.Contracts;
using Meteor.Employees.Core.Services.Validators;
using Meteor.Employees.Core.Services.Validators.Contracts;
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

builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<ExceptionsHandlingInterceptor>();
});
builder.Services.AddGrpcReflection();

builder.Services.AddHostedService<MigrationsRunner>();
builder.Services.AddScoped<ICustomersClient, GrpcCustomersClient>();
builder.Services.AddScoped<ICustomerDataAccessor, CustomerDataAccessor>();
builder.Services.AddScoped<IEmployeesService, EmployeesService>();
builder.Services.AddScoped<IValidator<Employee>, EmployeeModelValidator>();
builder.Services.AddScoped<IValidator<Employee>, EmployeeUniqueConstraintsValidator>();
builder.Services.AddSingleton<ICustomerIdProvider, HeadersCustomerIdProvider>();
builder.Services.AddScoped<CurrentCustomerDataPropagationMiddleware>();

builder.Services.AddHasher(options =>
{
    options.IterationsCount = builder.Configuration.GetValue<int>("Security:Hashing:IterationsCount");
    options.DefaultSaltLenght = builder.Configuration.GetValue<int>("Security:Hashing:DefaultSaltLenght");
    options.RequestBytesLength = builder.Configuration.GetValue<int>("Security:Hashing:RequestBytesLength");
});

var config = new TypeAdapterConfig();
config.Apply(new ApiMappingsRegister());
config.Apply(new CoreMappingsRegister());
config.Apply(new InfrastructureMappingsRegister());
builder.Services.AddSingleton<IMapper>(new Mapper(config));

builder.Services.AddGrpcClient<CustomersService.CustomersServiceClient>(options =>
{
    var url = builder.Configuration.GetValue<string>("Routing:ControllerUrl") ?? string.Empty;
    options.Address = new Uri(url);
    options.ChannelOptionsActions.Add(opt => opt.Credentials = ChannelCredentials.Insecure);
});

/*
builder.Services.AddDbContext<EmployeesContext>(
    options =>
    {
        options
            .UseNpgsql(
                string.Empty,
                opt => opt.MigrationsAssembly("Meteor.Employees.Migrations")
            )
            .UseSnakeCaseNamingConvention()
            .EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
    }
);*/

builder.Services.AddDbContext<EmployeesContext>(
    (services, options) =>
    {
        var customerDataAccessor = services.GetRequiredService<ICustomerDataAccessor>();
        if (customerDataAccessor.Settings is null)
        {
            throw new MissingCustomerDataException(
                "Unable to connect to customer database. Customer settings are not set."
            );
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

app.UseMiddleware<CurrentCustomerDataPropagationMiddleware>();
app.MapGrpcReflectionService();
app.MapGrpcService<EmployeesGrpcService>();
app.Run();