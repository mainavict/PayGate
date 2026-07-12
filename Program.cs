using Microsoft.EntityFrameworkCore;
using PayGate.Data;
using PayGate.Services.Interfaces;
using PayGate.Services.Implementation;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// 1. Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Controllers, OpenAPI & HttpClient
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHttpClient(); 

// 3. Data Protection
builder.Services.AddDataProtection()
    .SetApplicationName("PayGate")
    .SetDefaultKeyLifetime(TimeSpan.FromDays(90));

// 4. Services
builder.Services.AddScoped<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IClientAppService, ClientAppService>();
builder.Services.AddHttpClient<IDarajaService, DarajaService>();
builder.Services.AddScoped<IPaymentService, PaymentService>(); 
// 5. CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextJs", policy =>
    {
        policy.WithOrigins("http://localhost:3000").AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("PayGate Payment Gateway API").WithTheme(ScalarTheme.BluePlanet);
    });
}

app.UseCors("AllowNextJs");
app.UseHttpsRedirection();
app.MapControllers();

app.Run();