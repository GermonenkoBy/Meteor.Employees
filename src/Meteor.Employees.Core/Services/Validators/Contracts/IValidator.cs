using System.ComponentModel.DataAnnotations;
using Meteor.Employees.Core.Models;

namespace Meteor.Employees.Core.Services.Validators.Contracts;

public interface IValidator<TModel>
{
    Task<bool> TryValidateAsync(TModel model, ICollection<ValidationResult> validationResults);
}