using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SprPhone.Models;

[Table("departments")]
public class Department
{
    [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Column("title"), Required]
    public string Title { get; set; } = null!;

    [Column("parent_id"), JsonPropertyName("parent_id")]
    public int? ParentId { get; set; }
}

public record DepartmentDto(string Title, [property:JsonPropertyName("parent_id")] int? ParentId);