using DocuMind.Core.Entities;
using DocuMind.Core.Interfaces.IRepo;
using DocuMind.Infrastructure.Extention;
using DocuMind.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


// ===================================================================
// DATABASE CONFIGURATION & REPOSITORY PATTERN
// ===================================================================
builder.Services.AddInfrastructure(builder.Configuration);


// ===================================================================
// JWT AUTHENTICATION
// ===================================================================
builder.Services.AddJwtAuthentication(builder.Configuration);

// ===================================================================
// CORS CONFIGURATION
// ===================================================================
builder.Services.AddCORSPolicy(builder.Configuration);


// ===================================================================
// HANGFIRE CONFIGURATION
// ===================================================================
builder.Services
    .AddHangfireConfiguration(builder.Configuration)
     .AddHangfireServerWithConfig(builder.Configuration);

var app = builder.Build();



// CORS must be before Authentication
app.UseCors("AllowFrontend");
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


app.Run();
