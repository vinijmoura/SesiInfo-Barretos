using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using SesiInfo.Api.Models;
using System.Net;
using System.Text.Json;

namespace SesiInfo.Api.Functions;

public class ItemFunctions
{
    private readonly ILogger<ItemFunctions> _logger;
    private static readonly List<Item> Items = new()
    {
        new Item { Id = 1, Name = "Item 1", Description = "First item" },
        new Item { Id = 2, Name = "Item 2", Description = "Second item" }
    };

    public ItemFunctions(ILogger<ItemFunctions> logger)
    {
        _logger = logger;
    }

    [Function("HealthCheck")]
    public async Task<HttpResponseData> HealthCheck(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")] HttpRequestData req)
    {
        _logger.LogInformation("Health check requested");

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow
        });

        return response;
    }

    [Function("GetItems")]
    public async Task<HttpResponseData> GetItems(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/items")] HttpRequestData req)
    {
        _logger.LogInformation("Getting all items");

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(Items);

        return response;
    }

    [Function("GetItemById")]
    public async Task<HttpResponseData> GetItemById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/items/{id:int}")] HttpRequestData req,
        int id)
    {
        _logger.LogInformation("Getting item with ID: {Id}", id);

        var item = Items.FirstOrDefault(i => i.Id == id);

        if (item == null)
        {
            var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
            return notFoundResponse;
        }

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(item);

        return response;
    }

    [Function("CreateItem")]
    public async Task<HttpResponseData> CreateItem(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/items")] HttpRequestData req)
    {
        _logger.LogInformation("Creating new item");

        var requestBody = await req.ReadAsStringAsync();
        var newItem = JsonSerializer.Deserialize<Item>(requestBody ?? "{}", new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (newItem == null)
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            return badRequest;
        }

        newItem.Id = Items.Any() ? Items.Max(i => i.Id) + 1 : 1;
        Items.Add(newItem);

        var response = req.CreateResponse(HttpStatusCode.Created);
        response.Headers.Add("Location", $"/api/items/{newItem.Id}");
        await response.WriteAsJsonAsync(newItem);

        return response;
    }

    [Function("UpdateItem")]
    public async Task<HttpResponseData> UpdateItem(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "api/items/{id:int}")] HttpRequestData req,
        int id)
    {
        _logger.LogInformation("Updating item with ID: {Id}", id);

        var item = Items.FirstOrDefault(i => i.Id == id);

        if (item == null)
        {
            var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
            return notFoundResponse;
        }

        var requestBody = await req.ReadAsStringAsync();
        var updatedItem = JsonSerializer.Deserialize<Item>(requestBody ?? "{}", new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (updatedItem == null)
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            return badRequest;
        }

        item.Name = updatedItem.Name;
        item.Description = updatedItem.Description;

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(item);

        return response;
    }

    [Function("DeleteItem")]
    public HttpResponseData DeleteItem(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "api/items/{id:int}")] HttpRequestData req,
        int id)
    {
        _logger.LogInformation("Deleting item with ID: {Id}", id);

        var item = Items.FirstOrDefault(i => i.Id == id);

        if (item == null)
        {
            var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
            return notFoundResponse;
        }

        Items.Remove(item);

        var response = req.CreateResponse(HttpStatusCode.NoContent);
        return response;
    }
}
