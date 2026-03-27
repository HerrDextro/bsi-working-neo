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
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            new List<string>()
        }
    });
});

// DB Stuff
var connectionString = builder.Configuration.GetConnectionString("DebugConnection");
var serverVersion = new MySqlServerVersion(new Version(8, 0, 25));

builder.Services.AddDbContext<CommContext>(options =>
{
    if (!string.IsNullOrEmpty(connectionString))
    {
        options.UseMySql(connectionString, serverVersion);
    }
});

builder.Services.AddScoped<ITokenGenerator, JWTTokenGenerator>();
builder.Services.AddScoped<IPasswordHash, BCryptHasher>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


string host = "https://clustercalls-cmhrjljf.livekit.cloud";
string? apiKey = builder.Configuration["LiveKit:ApiKey"];
string? apiSecret = builder.Configuration["LiveKit:ApiSecret"];

if (apiKey == null || apiSecret == null)
{
    throw new Exception("LiveKit api secrets not set");
}

builder.Services.AddSingleton(new RoomServiceClient(host, apiKey, apiSecret));

//Authentication service
builder.Services.AddAuthentication(options => //code copy pasted from some task we had half a year ago
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    var key = builder.Configuration.GetSection("JwtConfig:Key").Value;
    options.TokenValidationParameters = new TokenValidationParameters
    {

        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero 
    };
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CommContext>();
    dbContext.Database.Migrate(); // Apply all migrations
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapUserEndpoints();
app.MapCallEndpoints();

app.Run();
