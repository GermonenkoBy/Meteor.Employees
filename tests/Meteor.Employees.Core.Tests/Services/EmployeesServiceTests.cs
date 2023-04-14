using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using Mapster;
using MapsterMapper;
using Meteor.Common.Core.Exceptions;
using Meteor.Common.Cryptography.Abstractions;
using Meteor.Employees.Core.Dtos;
using Meteor.Employees.Core.Mapping;
using Meteor.Employees.Core.Models;
using Meteor.Employees.Core.Services;
using Meteor.Employees.Core.Services.Validators.Contracts;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Meteor.Employees.Core.Tests.Services;

[TestClass]
public class EmployeesServiceTests
{
    private const int TestEmployeeId = 1;

    private const string TestEmployeeEmail = "test@employee.com";

    private static readonly EmployeesContext Context;

    private static readonly EmployeesService Service;

    private static readonly Mock<IHasher> HasherMock = new();

    private static readonly Mock<IValidator<Employee>> ValidatorMock = new();

    static EmployeesServiceTests()
    {
        var contextOptions = new DbContextOptionsBuilder<EmployeesContext>()
            .UseInMemoryDatabase(nameof(EmployeesServiceTests))
            .Options;

        Context = new(contextOptions);

        var mapperConfig = new TypeAdapterConfig();
        mapperConfig.Apply(new CoreMappingsRegister());
        var mapper = new Mapper(mapperConfig);

        Service = new(Context, mapper, HasherMock.Object, new[] { ValidatorMock.Object });
    }

    [ClassInitialize]
    public static void Initialize(TestContext _)
    {
        Context.Employees.Add(new()
        {
            Id = TestEmployeeId,
            EmailAddress = TestEmployeeEmail,
            PhoneNumber = "1234567890",
            FirstName = "FirstName",
            LastName = "LastName"
        });

        Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
    }

    [TestCleanup]
    public void Cleanup()
    {
        Context.ChangeTracker.Clear();
    }

    [TestMethod]
    public async Task GetEmployeeById_Should_ReturnEmployee()
    {
        var employee = await Service.GetEmployeeAsync(TestEmployeeId);
        Assert.IsNotNull(employee);
        Assert.AreEqual(TestEmployeeId, employee.Id);
    }

    [TestMethod]
    public async Task GetEmployeeByEmail_Should_ReturnEmployee()
    {
        var employee = await Service.GetEmployeeAsync(TestEmployeeEmail);
        Assert.IsNotNull(employee);
        Assert.AreEqual(TestEmployeeEmail, employee.EmailAddress);
    }

    [TestMethod]
    [ExpectedException(typeof(MeteorNotFoundException))]
    public async Task GetEmployeeByIdThatDoesNotExist_Should_ThrowException()
    {
        await Service.GetEmployeeAsync(-1);
    }

    [TestMethod]
    [ExpectedException(typeof(MeteorNotFoundException))]
    public async Task GetEmployeeByEmailThatDoesNotExist_Should_ThrowException()
    {
        await Service.GetEmployeeAsync("qwe");
    }

    [TestMethod]
    public async Task CreateEmployee_Should_SaveEmployee()
    {
        var dto = new CreateEmployeeDto
        {
            EmailAddress = "create@employee.com",
            PhoneNumber = "1987654321",
            FirstName = "Test1",
            LastName = "Test2",
            Password = "1234",
            Activate = true,
        };

        var hash = Encoding.UTF8.GetBytes(dto.Password);
        var salt = RandomNumberGenerator.GetBytes(4);

        HasherMock
            .Setup(hasher => hasher.Hash(dto.Password))
            .Returns((hash, salt));

        ValidatorMock
            .Setup(v => v.TryValidateAsync(It.IsAny<Employee>(), It.IsAny<ICollection<ValidationResult>>()))
            .ReturnsAsync(true);

        var returnedEmployee = await Service.CreateEmployeeAsync(dto);

        Assert.AreEqual(dto.FirstName, returnedEmployee.FirstName);
        Assert.AreEqual(dto.LastName, returnedEmployee.LastName);
        Assert.AreEqual(dto.EmailAddress, returnedEmployee.EmailAddress);
        Assert.AreEqual(dto.PhoneNumber, returnedEmployee.PhoneNumber);
        Assert.IsTrue(returnedEmployee.PasswordHash.SequenceEqual(hash));
        Assert.IsTrue(returnedEmployee.PasswordSalt.SequenceEqual(salt));
    }
}