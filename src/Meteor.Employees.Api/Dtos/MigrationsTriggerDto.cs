namespace Meteor.Employees.Api.Dtos;

public record MigrationsTriggerDto
{
    public List<int> CustomerIds { get; set; } = new();
}