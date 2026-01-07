using System.Collections.Concurrent;
using ITTitans.ToDoManager.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace ITTitans.ToDoManager.Services;

internal class InMemoryTodoService : ITodoService
{
    private readonly ConcurrentDictionary<Guid, TodoItem> _items = new();
    private readonly ILogger<InMemoryTodoService> _logger;


    public InMemoryTodoService() : this(NullLogger<InMemoryTodoService>.Instance)
    {
    }

    public InMemoryTodoService(ILogger<InMemoryTodoService> logger)
    {
        _logger = logger ?? NullLogger<InMemoryTodoService>.Instance;
    }

    public IReadOnlyList<TodoItem> GetAll()
    {
        return _items.Values
            .OrderBy(i => i.IsDone)
            .ThenBy(i => i.DueDate)
            .ThenBy(i => i.Title)
            .ToList();
    }

    public TodoItem Add(TodoItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        // Ensure new ID and normalize fields
        item.Id = Guid.NewGuid();
        item.Title = item.Title?.Trim() ?? string.Empty;
        if (item.DueDate == default)
        {
            item.DueDate = DateTime.Today;
        }
        _items[item.Id] = item;
        _logger.LogInformation("Todo created: {TodoId}", item.Id);
        return item;
    }

    public bool ToggleDone(Guid id)
    {
        if (_items.TryGetValue(id, out var existing))
        {
            var updated = new TodoItem
            {
                Id = existing.Id,
                Title = existing.Title,
                Description = existing.Description,
                DueDate = existing.DueDate,
                IsDone = !existing.IsDone
            };
            _items[id] = updated;
            _logger.LogInformation("Todo {Action}: {TodoId}", updated.IsDone ? "completed" : "reopened", id);
            return true;
        }
        return false;
    }

    public bool Remove(Guid id)
    {
        var removed = _items.TryRemove(id, out _);
        if (removed)
        {
            _logger.LogInformation("Todo removed: {TodoId}", id);
        }
        return removed;
    }

    public bool Update(TodoItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (!_items.ContainsKey(item.Id)) return false;
        // Normalize
        item.Title = item.Title?.Trim() ?? string.Empty;
        if (item.DueDate == default)
        {
            item.DueDate = DateTime.Today;
        }
        _items[item.Id] = item;
        _logger.LogInformation("Todo updated: {TodoId}", item.Id);
        return true;
    }

    public void ClearAll()
    {
        var count = _items.Count;
        _items.Clear();
        _logger.LogInformation("All todos removed: {Count}", count);
    }
}
