using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace SesiInfo.Api.Tests;

public class ApiEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ApiEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthCheck_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAllItems_ReturnsOkWithItems()
    {
        // Act
        var response = await _client.GetAsync("/api/items");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var items = await response.Content.ReadFromJsonAsync<List<Item>>();
        Assert.NotNull(items);
        Assert.NotEmpty(items);
    }

    [Fact]
    public async Task GetItemById_ExistingId_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/items/1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var item = await response.Content.ReadFromJsonAsync<Item>();
        Assert.NotNull(item);
        Assert.Equal(1, item.Id);
    }

    [Fact]
    public async Task GetItemById_NonExistingId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/items/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateItem_ReturnsCreated()
    {
        // Arrange
        var newItem = new Item
        {
            Name = "Test Item",
            Description = "Test Description"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/items", newItem);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var createdItem = await response.Content.ReadFromJsonAsync<Item>();
        Assert.NotNull(createdItem);
        Assert.Equal(newItem.Name, createdItem.Name);
        Assert.True(createdItem.Id > 0);
    }

    [Fact]
    public async Task UpdateItem_ExistingId_ReturnsOk()
    {
        // Arrange
        var updatedItem = new Item
        {
            Id = 1,
            Name = "Updated Item",
            Description = "Updated Description"
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/items/1", updatedItem);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var item = await response.Content.ReadFromJsonAsync<Item>();
        Assert.NotNull(item);
        Assert.Equal(updatedItem.Name, item.Name);
    }

    [Fact]
    public async Task UpdateItem_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        var updatedItem = new Item
        {
            Id = 999,
            Name = "Updated Item",
            Description = "Updated Description"
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/items/999", updatedItem);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteItem_ExistingId_ReturnsNoContent()
    {
        // Act
        var response = await _client.DeleteAsync("/api/items/2");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteItem_NonExistingId_ReturnsNotFound()
    {
        // Act
        var response = await _client.DeleteAsync("/api/items/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

// Model for testing
public record Item
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

