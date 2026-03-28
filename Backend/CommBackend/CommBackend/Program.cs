using CommBackend.Endpoints;
using CommBackend.Models.Context;
using CommBackend.Services.Hashing;
using CommBackend.Services.TokenGenerator;
using Livekit.Server.Sdk.Dotnet;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- CONFIGURATION & SECRETS ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DebugConnection' not found.");

var jwtKey = builder.Configuration["JwtConfig:Key"]
    ?? throw new InvalidOperationException("JWT Key not found in configuration.");

string host = "https://clustercalls-cmhrjljf.livekit.cloud";
string? apiKey = builder.Configuration["LiveKit:ApiKey"];
string? apiSecret = builder.Configuration["LiveKit:ApiSecret"];

if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
{
    throw new Exception("LiveKit api secrets are not set in configuration.");
}

// Fixed the console output syntax
Console.WriteLine($"LiveKit initialized with Key: {apiKey}");

// --- SERVICES REGISTRATION ---

// Database
var serverVersion = new MySqlServerVersion(new Version(8, 0, 25));
builder.Services.AddDbContext<CommContext>(options =>
{
    options.UseMySql(connectionString, serverVersion, mysqlOptions =>
    {
        mysqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null);
    });
});

// Auth & Security
builder.Services.AddScoped<ITokenGenerator, JWTTokenGenerator>();
builder.Services.AddScoped<IPasswordHash, BCryptHasher>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// External Services
builder.Services.AddSingleton(new RoomServiceClient(host, apiKey, apiSecret));

// API Infrastructure
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();

// Swagger (Merged into one block)
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Music Backend", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "Bearer Authentication with JWT Token",
        Type = SecuritySchemeType.Http
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Id = "Bearer", Type = ReferenceType.SecurityScheme }
            },
            new List<string>()
        }
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("Any", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// --- APP BUILD ---

var app = builder.Build();

// Automated Migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CommContext>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors("Any");

app.UseAuthentication();
app.UseAuthorization();

// Route Mappings
app.MapUserEndpoints();
app.MapCallEndpoints();
app.MapTeamsEndpoints();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();