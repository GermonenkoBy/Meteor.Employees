namespace Meteor.Employees.Core.Models;

public class Customer
{
    public int Id { get; set; }

    public string Domain { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
}