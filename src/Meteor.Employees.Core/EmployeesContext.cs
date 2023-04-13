using Meteor.Employees.Core.Models;
using Meteor.Employees.Core.Models.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement;

namespace Meteor.Employees.Core;

public class EmployeesContext : DbContext
{
    private readonly IFeatureManager _featureManager;

    public EmployeesContext(DbContextOptions<EmployeesContext> options, IFeatureManager featureManager) : base(options)
    {
        _featureManager = featureManager;
    }

    public DbSet<Employee> Employees => Set<Employee>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new EmployeeEntityTypeConfiguration());
    }
}