using System.ComponentModel.DataAnnotations;

namespace Meteor.Employees.Core.DataAnnotations;

public class MeteorPhoneNumberAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        var stringValue = value?.ToString();
        if (stringValue is null)
        {
            // This should be handled by RequiredAttribute
            return true;
        }

        return stringValue.All(char.IsDigit) && (stringValue.Length > 9 || stringValue.Length < 14);
    }
}