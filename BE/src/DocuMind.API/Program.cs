using DocuMind.Core.Entities;
using DocuMind.Core.Interfaces.IRepo;
using DocuMind.Infrastructure.Extention;
using DocuMind.Infrastructure.Middleware;
using DocuMind.Infrastructure.Services;
using Microsoft.OpenApi.Models;

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


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DocuMind API",
        Version = "v1",
        Description = "AI-Powered Document Management & Analysis System",
        Contact = new OpenApiContact
        {
            Name = "DocuMind Team",
            Email = "support@documind.com"
        }
    });

    // JWT Bearer Authentication in Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "DocuMind API v1");
        c.RoutePrefix = string.Empty; // Swagger at root URL
    });
    app.UseDeveloperExceptionPage();
}


// CORS must be before Authentication
app.UseCors("AllowFrontend");
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("DocuMindCORSPolicy"); // CORS Middleware
app.UseAuthentication();

app.UseHangfireDashboardConfigured(); // Hangfire Dashboard Middleware
app.UseAuthorization(); 
app.MapControllers();


app.Run();
