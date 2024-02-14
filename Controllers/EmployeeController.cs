using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
using Npgsql;
// using SprPhone.Data;
using SprPhone.Models;
using System.Data;

namespace SprPhone.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly ILogger<EmployeesController> _logger;
    // private readonly ApiDbContext _context;
    private readonly NpgsqlConnection _connection;

    public EmployeesController(
        ILogger<EmployeesController> logger,
        // ApiDbContext context,
        NpgsqlConnection connection
        )
    {
        _logger = logger;
        // _context = context;
        _connection = connection;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
    {
        // return await _context.Employees.OrderBy(employee => employee.Id).ToListAsync();

        var query = "SELECT * FROM employees ORDER BY id";
        var results = new List<Employee>();
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
                        Post = row["post"].ToString()!,
                        Phone = Convert.ToInt64(row["phone"]),
                        DepartmentId = Convert.ToInt32(row["department_id"])
                    });
                }
            }
        }
        
        return results;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Employee?>> GetEmployee(int id)
    {
        // var employee = await _context.Employees.FindAsync(id);
        //
        // if (employee == null)
        // {
        //     return NotFound();
        // }
        //
        // return employee;

        var query = $"SELECT * FROM employees WHERE id = {id};";
        try
        {
            await using (var command = new NpgsqlCommand(query, _connection))
            {
                await using (var reader = command.ExecuteReader())
                {
                    var dataTable = new DataTable();
                    dataTable.Load(reader);
                    return dataTable.Rows.Count switch
                    {
                        0 => NotFound("Record with this id does not exists!"),
                        _ => Ok(new Employee
                        {
                            Id = Convert.ToInt32(dataTable.Rows[0]["id"]),
                            Name = dataTable.Rows[0]["name"].ToString()!,
                            Surname = dataTable.Rows[0]["surname"].ToString()!,
                            Post = dataTable.Rows[0]["post"].ToString()!,
                            Phone = Convert.ToInt64(dataTable.Rows[0]["phone"]),
                            DepartmentId = Convert.ToInt32(dataTable.Rows[0]["department_id"])
                        })
                    };
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpPost]
    public async Task<ActionResult<Employee>> PostEmployee(EmployeeDto employee)
    {
        // _context.Employees.Add(employee);
        // await _context.SaveChangesAsync();
        //
        // return CreatedAtAction("GetEmployee", new { id = department.Id }, department);

        if (employee.DepartmentId == 0)
        {
            return BadRequest("Invalid department_ID");
        }
        var query = """
                    INSERT INTO employees(name, surname, post, phone, department_id)
                    VALUES (@name, @surname, @post, @phone, @department_id)
                    RETURNING id
                    """;
        try
        {
            await using (var command = new NpgsqlCommand(query, _connection))
            {
                command.Parameters.AddWithValue("@name", employee.Name);
                command.Parameters.AddWithValue("@surname", employee.Surname);
                command.Parameters.AddWithValue("@post", employee.Post);
                command.Parameters.AddWithValue("@phone", employee.Phone);
                command.Parameters.AddWithValue("@department_id", employee.DepartmentId);

                int id = (int)(await command.ExecuteScalarAsync() ?? throw new InvalidOperationException());

                return Ok(new Employee
                {
                    Id = id, Name = employee.Name, Surname = employee.Surname, Post = employee.Post,
                    Phone = employee.Phone, DepartmentId = employee.DepartmentId
                });
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutEmployee(int id, EmployeeDto employee)
    {
        // if (id != employee.Id)
        // {
        //     return BadRequest();
        // }
        //
        // _context.Entry(employee).State = EntityState.Modified;
        //
        // try
        // {
        //     await _context.SaveChangesAsync();
        // }
        // catch (DbUpdateConcurrencyException)
        // {
        //     if (!EmployeeExists(id))
        //     {
        //         return NotFound();
        //     }
        //
        //     throw;
        // }
        //
        // return NoContent();

        if (employee.DepartmentId == 0)
        {
            return BadRequest("Invalid department_ID");
        }
        if (EmployeeExists(id))
        {
            var query = $"""
                         UPDATE employees
                         SET name=@name, surname=@surname, post=@post, phone=@phone, department_id=@department_id
                         WHERE id = {id};
                         """;
            try
            {
                await using (var command = new NpgsqlCommand(query, _connection))
                {
                    command.Parameters.AddWithValue("@name", employee.Name);
                    command.Parameters.AddWithValue("@surname", employee.Surname);
                    command.Parameters.AddWithValue("@post", employee.Post);
                    command.Parameters.AddWithValue("@phone", employee.Phone);
                    command.Parameters.AddWithValue("@department_id", employee.DepartmentId);

                    await command.ExecuteScalarAsync();

                    return Ok("The record has been updated.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        return NotFound("Record with this id does not exists!");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        // var employee = await _context.Employees.FindAsync(id);
        //
        // if (employee == null)
        // {
        //     return NotFound();
        // }
        //
        // _context.Employees.Remove(employee);
        // await _context.SaveChangesAsync();
        //
        // return NoContent();

        if (EmployeeExists(id))
        {
            var query = $"DELETE FROM employees WHERE id = {id}";
            using (var command = new NpgsqlCommand(query, _connection))
            {
                await command.ExecuteScalarAsync();
        
                return Ok("Record has been deleted.");
            }
        }

        return NotFound("Record with this id does not exists!");
    }

    [HttpGet("Search")]
    public async Task<ActionResult<IEnumerable<Employee?>>> GetEmployeesByString(string search)
    {
        var query = $"""
                     SELECT *
                     FROM employees
                     WHERE
                         name iLIKE '%{search}%'
                         OR
                         surname iLIKE '%{search}%'
                         OR
                         post iLIKE '%{search}%'
                         OR
                         CAST(phone AS TEXT) iLIKE '%{search}%'
                     ORDER BY id
                     """;
        var results = new List<Employee>();
        await using (var command = new NpgsqlCommand(query, _connection))
        {
            await using (var reader = command.ExecuteReader())
            {
                var dataTable = new DataTable();
                dataTable.Load(reader);
                if (dataTable.Rows.Count == 0) return NotFound("There aren't employees at your request.");
                foreach (DataRow row in dataTable.Rows)
                {
                    results.Add(new Employee
                    {
                        Id = Convert.ToInt32(row["id"]),
                        Name = row["name"].ToString()!,
                        Surname = row["surname"].ToString()!,
                        Post = row["post"].ToString()!,
                        Phone = Convert.ToInt64(row["phone"]),
                        DepartmentId = Convert.ToInt32(row["department_id"])
                    });
                }
            }
        }
        
        return results;
    }

    private bool EmployeeExists(int id)
    {
        // return _context.Employees.Any(e => e.Id == id);

        var query = $"SELECT COUNT(id) FROM employees WHERE id = {id}";
        long count = 0;
        try
        {
            using (var command = new NpgsqlCommand(query, _connection))
            {
                count = (long) (command.ExecuteScalar() ?? throw new InvalidOperationException());
            }

            return Convert.ToBoolean(count);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}