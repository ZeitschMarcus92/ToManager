using ITTitans.ToDoManager.Data;
using ITTitans.ToDoManager.Models;
using Microsoft.EntityFrameworkCore;

namespace ITTitans.ToDoManager.Services;

internal class PostgresTodoService : ITodoService
{
    private readonly TodoDbContext _db;
    private readonly ILogger<PostgresTodoService> _logger;


    public PostgresTodoService(TodoDbContext db, ILogger<PostgresTodoService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public IReadOnlyList<TodoItem> GetAll()
    {
        return _db.Todos
            .AsNoTracking()
            .OrderBy(i => i.IsDone)
            .ThenBy(i => i.DueDate)
            .ThenBy(i => i.Title)
            .ToList();
    }

    public TodoItem Add(TodoItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        item.Id = Guid.NewGuid();
        item.Title = item.Title?.Trim() ?? string.Empty;
        if (item.DueDate == default)
        {
            item.DueDate = DateTime.UtcNow.Date;
        }
        else
        {
            item.DueDate = item.DueDate.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(item.DueDate, DateTimeKind.Utc)
                : item.DueDate.ToUniversalTime();
        }

        _db.Todos.Add(item);
        _db.SaveChanges();
        _logger.LogInformation("Todo created in Postgres: {TodoId}", item.Id);
        return item;
    }

    public bool ToggleDone(Guid id)
    {
        var existing = _db.Todos.FirstOrDefault(t => t.Id == id);
        if (existing == null) return false;
        existing.IsDone = !existing.IsDone;
        _db.SaveChanges();
        _logger.LogInformation("Todo {Action} in Postgres: {TodoId}", existing.IsDone ? "completed" : "reopened", id);
        return true;
    }

    public bool Remove(Guid id)
    {
        var existing = _db.Todos.FirstOrDefault(t => t.Id == id);
        if (existing == null) return false;
        _db.Todos.Remove(existing);
        _db.SaveChanges();
        _logger.LogInformation("Todo removed in Postgres: {TodoId}", id);
        return true;
    }

    public bool Update(TodoItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        var existing = _db.Todos.AsNoTracking().FirstOrDefault(t => t.Id == item.Id);
        if (existing == null) return false;
        item.Title = item.Title?.Trim() ?? string.Empty;
        if (item.DueDate == default)
        {
            item.DueDate = DateTime.UtcNow.Date;
        }
        else
        {
            item.DueDate = item.DueDate.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(item.DueDate, DateTimeKind.Utc)
                : item.DueDate.ToUniversalTime();
        }
        _db.Todos.Update(item);
        _db.SaveChanges();
        _logger.LogInformation("Todo updated in Postgres: {TodoId}", item.Id);
        return true;
    }

    public void ClearAll()
    {
        var count = _db.Todos.ExecuteDelete();
        _logger.LogInformation("All todos removed in Postgres: {Count}", count);
    }
}
