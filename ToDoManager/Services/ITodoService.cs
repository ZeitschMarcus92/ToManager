using ITTitans.ToDoManager.Models;

namespace ITTitans.ToDoManager.Services;

internal interface ITodoService
{
    IReadOnlyList<TodoItem> GetAll();
    TodoItem Add(TodoItem item);
    bool ToggleDone(Guid id);
    bool Remove(Guid id);
    bool Update(TodoItem item);
    void ClearAll();
}
