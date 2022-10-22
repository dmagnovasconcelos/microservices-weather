var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/observation/{zip}", (string zip) => {
    return Results.Ok(zip);
});

app.Run();