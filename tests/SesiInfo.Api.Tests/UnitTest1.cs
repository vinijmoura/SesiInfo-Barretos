using SesiInfo.Api.Models;

namespace SesiInfo.Api.Tests;

public class ItemModelTests
{
    [Fact]
    public void Item_CanBeCreated()
    {
        // Arrange & Act
        var item = new Item
        {
            Id = 1,
            Name = "Test Item",
            Description = "Test Description"
        };

        // Assert
        Assert.Equal(1, item.Id);
        Assert.Equal("Test Item", item.Name);
        Assert.Equal("Test Description", item.Description);
    }

    [Fact]
    public void Item_DefaultsToEmptyStrings()
    {
        // Arrange & Act
        var item = new Item();

        // Assert
        Assert.Equal(string.Empty, item.Name);
        Assert.Equal(string.Empty, item.Description);
    }

    [Fact]
    public void Item_RecordEqualityWorks()
    {
        // Arrange
        var item1 = new Item { Id = 1, Name = "Test", Description = "Desc" };
        var item2 = new Item { Id = 1, Name = "Test", Description = "Desc" };

        // Act & Assert
        Assert.Equal(item1, item2);
    }
}

public class ItemLogicTests
{
    [Fact]
    public void ItemId_CanBeSet()
    {
        // Arrange
        var item = new Item { Name = "Test" };

        // Act
        item.Id = 5;

        // Assert
        Assert.Equal(5, item.Id);
    }

    [Fact]
    public void ItemName_CanBeUpdated()
    {
        // Arrange
        var item = new Item { Id = 1, Name = "Original" };

        // Act
        item.Name = "Updated";

        // Assert
        Assert.Equal("Updated", item.Name);
    }

    [Fact]
    public void ItemDescription_CanBeUpdated()
    {
        // Arrange
        var item = new Item { Id = 1, Description = "Original" };

        // Act
        item.Description = "Updated";

        // Assert
        Assert.Equal("Updated", item.Description);
    }
}

