using System.ComponentModel.DataAnnotations;
using Meteor.Employees.Core.DataAnnotations;
using Meteor.Employees.Core.Models.Enums;

namespace Meteor.Employees.Core.Models;

public class Employee
{
    public int Id { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "First name is required.")]
    [StringLength(50, ErrorMessage = "Max first name length is 50.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessage = "Last name is required.")]
    [StringLength(50, ErrorMessage = "Max last name length is 50.")]
    public string LastName { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessage = "Email address is required.")]
    [StringLength(250, ErrorMessage = "Max email address length is 250.")]
    [EmailAddress(ErrorMessage = "Must be a valid email address.")]
    public string EmailAddress { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessage = "Phone number is required.")]
    [MeteorPhoneNumber(ErrorMessage = "Must contain only digits.")]
    public string PhoneNumber { get; set; } = string.Empty;

    public EmployeeStatuses Status { get; set; }

    [Required(ErrorMessage = "Password hash is required.")]
    [MaxLength(256, ErrorMessage = "Max password hash length is 256.")]
    public byte[] PasswordHash { get; set; } = Array.Empty<byte>();

    [Required(ErrorMessage = "Password salt is required.")]
    [MaxLength(256, ErrorMessage = "Max password salt length is 256.")]
    public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
}