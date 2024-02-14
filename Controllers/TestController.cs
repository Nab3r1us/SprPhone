using System.Data;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Newtonsoft.Json;
// using SprPhone.Data;
using SprPhone.Models;


namespace SprPhone.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly ILogger<TestController> _logger;
    // private readonly ApiDbContext _context;
    private readonly NpgsqlConnection _connection;

    public TestController(
        ILogger<TestController> logger,
        // ApiDbContext context,
        NpgsqlConnection connection)
    {
        _logger = logger;
        // _context = context;
        _connection = connection;
    }

    [HttpGet]
    public async Task<ActionResult<List<Employee>>> Test()
    {
        var query = "SELECT * FROM employees";
        var results = new List<Employee>();
        try
        {
            await using (var command = new NpgsqlCommand(query, _connection))
            {
                await using (var reader = command.ExecuteReader())
                {
                    var dataTable = new DataTable();
                    dataTable.Load(reader);
                    foreach (DataRow row in dataTable.Rows)
                    {
                        results.Add(new Employee
                        {
                            Id = Convert.ToInt32(row["id"]),
                            Name = row["name"].ToString()!,
                            Surname = row["surname"].ToString()!,
                            Post = row["surname"].ToString()!,
                            DepartmentId = Convert.ToInt32(row["department_id"]),
                            Phone = Convert.ToInt64(row["phone"])
                        });
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return results;
    }
    
    [HttpGet("RawQuery")]
    public string RawQuery(string query)
    {
        string result = String.Empty;
        
        try
        {
            using (_connection)
            {
                using (var command = new NpgsqlCommand(query, _connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        var dataTable = new DataTable();
                        dataTable.Load(reader);
                        result = JsonConvert.SerializeObject(dataTable);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        return result;
    }

    [HttpGet("FillDemoData")]
    public void FillDemoData()
    {
        /*
        List<Department> demoDepartments =
        [
            new Department() { Id = 1, Title = "Отдел разработки", ParentId = null },
            new Department() { Id = 2, Title = "Отдел маркетинга", ParentId = null },
            new Department() { Id = 3, Title = "Отдел продаж", ParentId = 2 },
            new Department() { Id = 4, Title = "Отдел технической поддержки", ParentId = 1 },
            new Department() { Id = 5, Title = "Отдел продаж оптовых клиентов", ParentId = 3 },
            new Department() { Id = 6, Title = "Отдел продаж розничных клиентов", ParentId = 3 }
        ];
        List<Employee> demoEmployees =
        [
            new Employee()
            {
                Id = 1, Name = "Иван", Surname = "Иванов", Post = "Разработчик", DepartmentId = 1, Phone = 375251234567
            },
            new Employee()
            {
                Id = 2, Name = "Петр", Surname = "Петров", Post = "Менеджер по маркетингу", DepartmentId = 2,
                Phone = 375291234567
            },
            new Employee()
            {
                Id = 3, Name = "Сидор", Surname = "Сидоров", Post = "Продавец", DepartmentId = 3, Phone = 375293123456
            },
            new Employee()
            {
                Id = 4, Name = "Елена", Surname = "Еленова", Post = "Технический специалист", DepartmentId = 4,
                Phone = 375296123456
            },
            new Employee()
            {
                Id = 5, Name = "Алексей", Surname = "Алексеев", Post = "Продавец оптовых клиентов", DepartmentId = 5,
                Phone = 375333123456
            },
            new Employee()
            {
                Id = 6, Name = "Мария", Surname = "Мариева", Post = "Продавец розничных клиентов", DepartmentId = 6,
                Phone = 375442123456
            }
        ];
        try
        {
            foreach (var demoDepartment in demoDepartments)
            {
                var department =
                    _context.Departments.FirstOrDefault(department => department.Title == demoDepartment.Title);
                if (department != null) continue;
                _context.Departments.Add(demoDepartment);
                _context.SaveChanges();
            }

            foreach (var demoEmployee in demoEmployees)
            {
                var employee =
                    _context.Employees.FirstOrDefault(employee =>
                        employee.Name == demoEmployee.Name && employee.Surname == demoEmployee.Surname);
                if (employee != null) continue;
                _context.Employees.Add(demoEmployee);
                _context.SaveChanges();
            }

            return "Employees added!";
        }
        catch (Exception e)
        {
            return $"{e.GetType().FullName}\n---\n{e.Message}";
        }
        */
    }

}