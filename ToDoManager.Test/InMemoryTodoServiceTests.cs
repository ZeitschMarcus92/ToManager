using ITTitans.ToDoManager.Models;
using ITTitans.ToDoManager.Services;
using Xunit;

namespace ToDoManager.Test;

public class InMemoryTodoServiceTests
{
    private static InMemoryTodoService CreateService() => new();

    [Fact]
    public void Add_ShouldAssignNewId_And_NormalizeFields()
    {
        var svc = CreateService();
        var item = new TodoItem
        {
            Id = Guid.Empty,
            Title = "  Title with spaces  ",
            Description = "Desc",
            DueDate = default
        };

        var added = svc.Add(item);

        Assert.NotEqual(Guid.Empty, added.Id);
        Assert.Equal("Title with spaces", added.Title);
        Assert.Equal(DateTime.Today, added.DueDate);

        var all = svc.GetAll();
        Assert.Single(all);
        Assert.Equal(added.Id, all[0].Id);
    }

    [Fact]
    public void ToggleDone_ShouldToggle_And_ReturnTrue_WhenExists_ElseFalse()
    {
        var svc = CreateService();
        var added = svc.Add(new TodoItem { Title = "a" });

        var ok1 = svc.ToggleDone(added.Id);
        var after1 = svc.GetAll().Single(i => i.Id == added.Id);
        var ok2 = svc.ToggleDone(added.Id);
        var after2 = svc.GetAll().Single(i => i.Id == added.Id);
        var okMissing = svc.ToggleDone(Guid.NewGuid());

        Assert.True(ok1);
        Assert.True(after1.IsDone);
        Assert.True(ok2);
        Assert.False(after2.IsDone);
        Assert.False(okMissing);
    }

    [Fact]
    public void Remove_ShouldDelete_And_ReturnTrue_WhenExists_ElseFalse()
    {
        var svc = CreateService();
        var a = svc.Add(new TodoItem { Title = "a" });

        var ok1 = svc.Remove(a.Id);
        var ok2 = svc.Remove(a.Id);

        Assert.True(ok1);
        Assert.False(ok2);
        Assert.Empty(svc.GetAll());
    }

    [Fact]
    public void Update_ShouldNormalize_And_ReturnFalse_WhenMissing()
    {
        var svc = CreateService();
        var a = svc.Add(new TodoItem { Title = "a" });

        // Update existing
        var updated = new TodoItem
        {
            Id = a.Id,
            Title = "  x  ",
            Description = " d ",
            DueDate = default,
            IsDone = true
        };
        var okExisting = svc.Update(updated);
        var fetched = svc.GetAll().Single(i => i.Id == a.Id);

        // Update missing
        var okMissing = svc.Update(new TodoItem { Id = Guid.NewGuid(), Title = "z" });

        Assert.True(okExisting);
        Assert.Equal("x", fetched.Title);
        Assert.Equal(DateTime.Today, fetched.DueDate);
        Assert.True(fetched.IsDone);
        Assert.False(okMissing);
    }

    [Fact]
    public void GetAll_ShouldBeSorted_ByIsDone_ThenDueDate_ThenTitle()
    {
        var svc = CreateService();
        svc.Add(new TodoItem { Title = "b", DueDate = DateTime.Today.AddDays(1), }); // 2
        svc.Add(new TodoItem { Title = "a", DueDate = DateTime.Today.AddDays(1), }); // 1
        svc.Add(new TodoItem { Title = "c", DueDate = DateTime.Today.AddDays(1), IsDone = true }); // 5
        svc.Add(new TodoItem { Title = "a", DueDate = DateTime.Today.AddDays(2), }); // 3
        svc.Add(new TodoItem { Title = "a", DueDate = DateTime.Today, }); // 0
        svc.Add(new TodoItem { Title = "a", DueDate = DateTime.Today.AddDays(2), IsDone = true }); // 4

        var all = svc.GetAll();
        var titles = all.Select(i => ($"{(i.IsDone ? 1 : 0)}|{i.DueDate:yyyyMMdd}|{i.Title}"));

        var arr = titles.ToArray();
        Assert.Collection(arr,
            s => Assert.StartsWith("0|", s, StringComparison.Ordinal),
            s => Assert.StartsWith("0|", s, StringComparison.Ordinal),
            s => Assert.StartsWith("0|", s, StringComparison.Ordinal),
            s => Assert.StartsWith("0|", s, StringComparison.Ordinal),
            s => Assert.StartsWith("1|", s, StringComparison.Ordinal),
            s => Assert.StartsWith("1|", s, StringComparison.Ordinal)
        );

        // Spot-check first few are properly ordered by DueDate then Title
        Assert.EndsWith("|a", arr[0], StringComparison.Ordinal);
        Assert.EndsWith("|a", arr[1], StringComparison.Ordinal);
        Assert.True(string.CompareOrdinal(arr[0], arr[1]) <= 0);
    }

    [Fact]
    public void ClearAll_ShouldRemoveEverything()
    {
        var svc = CreateService();
        svc.Add(new TodoItem { Title = "a" });
        svc.Add(new TodoItem { Title = "b" });

        Assert.NotEmpty(svc.GetAll());
        svc.ClearAll();
        Assert.Empty(svc.GetAll());
    }
}
