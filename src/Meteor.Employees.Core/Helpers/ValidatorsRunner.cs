using System.ComponentModel.DataAnnotations;
using Meteor.Common.Core.Exceptions;
using Meteor.Employees.Core.Services.Validators.Contracts;

namespace Meteor.Employees.Core.Helpers;

public static class ValidatorsRunner
{
    private const int DefaultErrorsListCapacity = 10;

    public static async Task EnsureIsValid<TModel>(
        TModel model,
        IEnumerable<IValidator<TModel>> validators,
        string errorMessage = "The model is invalid",
        bool runAll = true
    )
    {
        var valid = true;
        var errors = new List<ValidationResult>(DefaultErrorsListCapacity);

        foreach (var validator in validators)
        {
            if (!await validator.TryValidateAsync(model, errors))
            {
                valid = false;

                if (!runAll)
                {
                    break;
                }
            }
        }

        if (!valid)
        {
            throw new MeteorValidationException(errorMessage)
            {
                Errors = errors,
            };
        }
    }
}