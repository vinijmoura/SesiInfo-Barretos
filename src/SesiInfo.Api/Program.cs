var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
    .WithName("HealthCheck")
    .WithTags("Health");

// Sample data
var items = new List<Item>
{
    new Item { Id = 1, Name = "Item 1", Description = "First item" },
    new Item { Id = 2, Name = "Item 2", Description = "Second item" }
};

// GET all items
app.MapGet("/api/items", () => Results.Ok(items))
    .WithName("GetItems")
    .WithTags("Items")
    .Produces<List<Item>>(StatusCodes.Status200OK);

// GET item by id
app.MapGet("/api/items/{id:int}", (int id) =>
{
    var item = items.FirstOrDefault(i => i.Id == id);
    return item is not null ? Results.Ok(item) : Results.NotFound();
})
    .WithName("GetItemById")
    .WithTags("Items")
    .Produces<Item>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);

// POST new item
app.MapPost("/api/items", (Item item) =>
{
    item.Id = items.Max(i => i.Id) + 1;
    items.Add(item);
    return Results.Created($"/api/items/{item.Id}", item);
})
    .WithName("CreateItem")
    .WithTags("Items")
    .Produces<Item>(StatusCodes.Status201Created);

// PUT update item
app.MapPut("/api/items/{id:int}", (int id, Item updatedItem) =>
{
    var item = items.FirstOrDefault(i => i.Id == id);
    if (item is null)
        return Results.NotFound();

    item.Name = updatedItem.Name;
    item.Description = updatedItem.Description;
    return Results.Ok(item);
})
    .WithName("UpdateItem")
    .WithTags("Items")
    .Produces<Item>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);

// DELETE item
app.MapDelete("/api/items/{id:int}", (int id) =>
{
    var item = items.FirstOrDefault(i => i.Id == id);
    if (item is null)
        return Results.NotFound();

    items.Remove(item);
    return Results.NoContent();
})
    .WithName("DeleteItem")
    .WithTags("Items")
    .Produces(StatusCodes.Status204NoContent)
    .Produces(StatusCodes.Status404NotFound);

app.Run();

// Model
public record Item
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

// Make Program accessible for testing
public partial class Program { }

