using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SprPhone.Models;

[Table("employees")]
public class Employee
{
    [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Column("name")]
    public string Name { get; set; } = null!;
    [Column("surname")]
    public string Surname { get; set; } = null!;
    [Column("post")]
    public string Post { get; set; } = null!;
    [Column("department_id"), JsonPropertyName("department_id")]
    public int DepartmentId { get; set; }
    [Column("phone")]
    public long Phone { get; set; }
}

public record EmployeeDto(
    string Name, string Surname, string Post, [property:JsonPropertyName("department_id")] int DepartmentId, long Phone
    );