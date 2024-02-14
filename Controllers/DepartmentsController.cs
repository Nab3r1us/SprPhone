using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
using Npgsql;
// using SprPhone.Data;
using SprPhone.Extensions;
using SprPhone.Models;
using System.Data;

namespace SprPhone.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepartmentsController : ControllerBase
{
    private readonly ILogger<DepartmentsController> _logger;
    // private readonly ApiDbContext _context;
    private readonly NpgsqlConnection _connection;

    public DepartmentsController(
        ILogger<DepartmentsController> logger,
        // ApiDbContext context,
        NpgsqlConnection connection
        )
    {
        _logger = logger;
        // _context = context;
        _connection = connection;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Department>>> GetDepartments()
    {
        // return await _context.Departments.OrderBy(department => department.Id).ToListAsync();

        var query = "SELECT * FROM departments ORDER BY id;";
        var results = new List<Department>();
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
                        results.Add(new Department()
                        {
                            Id = Convert.ToInt32(row["id"]),
                            Title = row["title"].ToString()!,
                            ParentId = row.TryGetInt("parent_id")
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

    [HttpGet("{id}")]
    public async Task<ActionResult<Department>> GetDepartment(int id)
    {
        // var department = await _context.Departments.FindAsync(id);
        //
        // if (department == null)
        // {
        //     return NotFound();
        // }
        //
        // return department;

        var query = $"SELECT * FROM departments WHERE id = {id};";
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
                        _ => Ok(new Department
                        {
                            Id = Convert.ToInt32(dataTable.Rows[0]["id"]),
                            Title = dataTable.Rows[0]["title"].ToString()!,
                            ParentId = dataTable.Rows[0].TryGetInt("parent_id")
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
    public async Task<ActionResult<Department>> PostDepartment(DepartmentDto department)
    {
        // department.ParentId = department.ParentId == 0 ? null : department.ParentId;
        // _context.Departments.Add(department);
        // await _context.SaveChangesAsync();
        //
        // return CreatedAtAction("GetDepartment", new { id = department.Id }, department);

        var query = "INSERT INTO departments(title, parent_id) VALUES(@title, @parent_id) RETURNING id;";
        try
        {
            await using (var command = new NpgsqlCommand(query, _connection))
            {
                command.Parameters.AddWithValue("@title", department.Title);
                command.Parameters.AddWithValue("@parent_id",
                    department.ParentId.HasValue ? department.ParentId.Value : DBNull.Value);
                
                int id = (int)(await command.ExecuteScalarAsync() ?? throw new InvalidOperationException());

                return Ok(new Department { Id = id, Title = department.Title, ParentId = department.ParentId });
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutDepartment(int id, DepartmentDto department)
    {
        // if (id != department.Id) return BadRequest();
        //
        // _context.Entry(department).State = EntityState.Modified;
        //
        // try
        // {
        //     await _context.SaveChangesAsync();
        // }
        // catch (DbUpdateConcurrencyException)
        // {
        //     if (!DepartmentExists(id)) return NotFound();
        //
        //     throw;
        // }
        //
        // return NoContent();

        if (DepartmentExists(id))
        {
            var query = $"UPDATE departments SET title=@title, parent_id=@parent_id WHERE id = {id};";
            try
            {
                await using (var command = new NpgsqlCommand(query, _connection))
                {
                    command.Parameters.AddWithValue("@title", department.Title);
                    command.Parameters.AddWithValue("@parent_id",
                        department.ParentId.HasValue ? department.ParentId.Value : DBNull.Value);

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
    public async Task<IActionResult> DeleteDepartment(int id)
    {
        // var department = await _context.Departments.FindAsync(id);
        //
        // if (department == null)
        // {
        //     return NotFound();
        // }
        //
        // _context.Departments.Remove(department);
        // await _context.SaveChangesAsync();
        //
        // return NoContent();

        if (DepartmentExists(id))
        {
            var query = $"DELETE FROM departments WHERE id = {id}";
            using (var command = new NpgsqlCommand(query, _connection))
            {
                await command.ExecuteScalarAsync();
        
                return Ok("Record has been deleted.");
            }
        }

        return NotFound("Record with this id does not exists!");
    }

    [HttpGet("{id}/Employees")]
    public async Task<ActionResult<IEnumerable<Employee>>> GetEmployeesByDepartment(int id)
    {
        var query = $"SELECT * FROM employees WHERE department_id = {id} ORDER BY id";
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

    [HttpGet("{id}/Subdepartments")]
    public async Task<ActionResult<IEnumerable<Department>>> GetSubDepartmentById(int id)
    {
        var query = $"SELECT * FROM departments WHERE parent_id = {id} ORDER BY id";
        var results = new List<Department>();
        await using (var command = new NpgsqlCommand(query, _connection))
        {
            await using (var reader = command.ExecuteReader())
            {
                var dataTable = new DataTable();
                dataTable.Load(reader);
                foreach (DataRow row in dataTable.Rows)
                {
                    results.Add(new Department
                    {
                        Id = Convert.ToInt32(row["id"]),
                        Title = row["title"].ToString()!,
                        ParentId = row.TryGetInt("parent_id")
                    });
                }
            }
        }
        
        return results;
    }

    private bool DepartmentExists(int id)
    {
        // return _context.Departments.Any(e => e.Id == id);

        var query = $"SELECT COUNT(id) FROM departments WHERE id = {id}";
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
