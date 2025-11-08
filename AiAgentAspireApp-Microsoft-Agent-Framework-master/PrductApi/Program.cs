var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

string[] summaries = ["Product A", "Product B", "Product C", "Product D", "Product E", "Product F", "Product G", "Product H", "Product I", "Product J"];



app.MapGet("/products", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        // craete a product object list
        new Product(index, $"Product {index}", summaries[Random.Shared.Next(summaries.Length)])
        ).ToArray();
    return forecast;
})
.WithName("Getproduct");

app.Run();

record Product(int Id, string Name, string? Desc);

