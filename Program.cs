using Microsoft.EntityFrameworkCore;
using PayGate.Data;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Register Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Add Controllers
builder.Services.AddControllers();

// 3. Add OpenAPI (Built-in for .NET 9/10)
builder.Services.AddOpenApi();

var app = builder.Build();

// 4. Configure Pipeline
if (app.Environment.IsDevelopment())
{
    // Map the OpenAPI JSON endpoint
    app.MapOpenApi();
    
    // Map the Scalar UI
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("PayGate Payment Gateway")
            .WithTheme(ScalarTheme.BluePlanet);
    });
}

app.MapControllers();

app.Run();