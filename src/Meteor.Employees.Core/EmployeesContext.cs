using Meteor.Employees.Core.Models;
using Meteor.Employees.Core.Models.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Meteor.Employees.Core;

public class EmployeesContext : DbContext
{
    public EmployeesContext(DbContextOptions<EmployeesContext> options) : base(options)
    {
    }

    public DbSet<Employee> Employees => Set<Employee>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new EmployeeEntityTypeConfiguration());
    }
}