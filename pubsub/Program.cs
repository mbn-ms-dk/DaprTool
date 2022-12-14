using Dapr.Client;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDaprClient();

var app = builder.Build();
app.UseCloudEvents();

app.MapGet("/order", async (DaprClient client) =>
    await client.GetStateAsync<Order>("statestore", "orders"));

app.MapPost("/neworder", async (Order o, DaprClient client) =>
{
    await client.SaveStateAsync<Order>("statestore", "orders", o);
    return Results.Ok();
});


app.Run();

public record Order(string OrderId);
