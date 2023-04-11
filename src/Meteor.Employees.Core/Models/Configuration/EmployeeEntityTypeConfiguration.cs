using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meteor.Employees.Core.Models.Configuration;

public class EmployeeEntityTypeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.HasIndex(e => e.EmailAddress).IsUnique();
        builder.HasIndex(e => e.PhoneNumber).IsUnique();
    }
}