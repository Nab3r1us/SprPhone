using Microsoft.EntityFrameworkCore;
using SprPhone.Models;

namespace SprPhone.Data;

public class ApiDbContext(DbContextOptions<ApiDbContext> options) : DbContext(options)
{
    public DbSet<Department> Departments { get; set; } = null!;
    public DbSet<Employee> Employees { get; set; } = null!;
}