using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// builder.AddAzureCosmosDB("cosmos");
builder.AddSqlServerDbContext<MyDbContext>("sql");

// Add services to the container.
builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.MapDefaultEndpoints();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
});

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<MyDbContext>();
    context.Database.EnsureCreated();
}

app.MapGet("/sql/entry", async (MyDbContext ctx, CancellationToken ct) =>
{
    await ctx.Database.EnsureCreatedAsync(ct);
    
    var entry = new Entry();
    await ctx.Entries.AddAsync(entry, ct);
    await ctx.SaveChangesAsync(ct);

    var entries = await ctx.Entries.ToListAsync(cancellationToken: ct);

    return new
    {
        totalEntries = entries.Count,
        entries
    };
});

//app.MapGet("/cosmos/entry", async ([FromServices] CosmosClient cosmosClient) =>
//{
//    var db = (await cosmosClient.CreateDatabaseIfNotExistsAsync("db")).Database;
//    var container = (await db.CreateContainerIfNotExistsAsync("entries", "/Id")).Container;

//    // Add an entry to the database on each request.
//    var newEntry = new Entry() { Id = Guid.NewGuid().ToString() };
//    await container.CreateItemAsync(newEntry);

//    var entries = new List<Entry>();
//    var iterator = container.GetItemQueryIterator<Entry>(requestOptions: new QueryRequestOptions() { MaxItemCount = 5 });

//    var batchCount = 0;
//    while (iterator.HasMoreResults)
//    {
//        batchCount++;
//        var batch = await iterator.ReadNextAsync();
//        foreach (var entry in batch)
//        {
//            entries.Add(entry);
//        }
//    }

//    return new
//    {
//        batchCount,
//        totalEntries = entries.Count,
//        entries
//    };
//});

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

// Note: Regarding migrations: https://blog.peterritchie.com/posts/entity-framework-in-aspire
// and https://github.com/dotnet/aspire-samples/tree/main/samples/eShopLite
public class MyDbContext(DbContextOptions<MyDbContext> options) : DbContext(options)
{
    public DbSet<Entry> Entries { get; set; }
}

public class Entry
{
    [JsonProperty("id")]
    public string? Id { get; set; } = Guid.NewGuid().ToString();
}
