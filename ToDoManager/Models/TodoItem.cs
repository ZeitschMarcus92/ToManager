using System.ComponentModel.DataAnnotations;

namespace ITTitans.ToDoManager.Models;

internal class TodoItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime DueDate { get; set; } = DateTime.UtcNow.Date;

    public bool IsDone { get; set; }
}
