using Meteor.Employees.Core.DataAnnotations;

namespace Meteor.Employees.Core.Tests.DataAnnotations;

[TestClass]
public class MeteorPhoneNumberAttributeTests
{
    private readonly MeteorPhoneNumberAttribute _attribute = new();

    [TestMethod]
    [DataRow(null, true)]
    [DataRow("1", false)]
    [DataRow("12", false)]
    [DataRow("123", false)]
    [DataRow("1234", false)]
    [DataRow("12345", false)]
    [DataRow("123456", false)]
    [DataRow("1234567", false)]
    [DataRow("12345678", false)]
    [DataRow("123456789", false)]
    [DataRow("1234567890", true)]
    [DataRow("12345678901", true)]
    [DataRow("123456789012", true)]
    [DataRow("1234567890123", true)]
    [DataRow("12345678901234", false)]
    public void TestPhoneNumber_Should_ValidateCorrectly(string? phoneNumber, bool expectedResult)
    {
        var valid = _attribute.IsValid(phoneNumber);
        Assert.AreEqual(expectedResult, valid);
    }
}