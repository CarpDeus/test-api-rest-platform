using Serilog;
using TestApiRestPlatform.Middleware;
using TestApiRestPlatform.Authentication;
using FluentValidation;
using Microsoft.OpenApi;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Add Authentication
builder.Services.AddAuthentication("CustomScheme")
    .AddScheme<CustomAuthenticationOptions, CustomAuthenticationHandler>("CustomScheme", null);
builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "Test API Rest Platform", 
        Version = "v1",
        Description = "A simple API that returns specific HTTP status codes with authentication and validation capabilities"
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter the Authorization header value.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", null, null)] = new List<string>()
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Test API Rest Platform v1");
    c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
});

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Add custom request logging middleware
app.UseMiddleware<RequestLoggingMiddleware>();

app.MapControllers();

app.Run();
