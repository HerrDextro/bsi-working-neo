using CommBackend.Endpoints;
using CommBackend.Models.Context;
using Livekit.Server.Sdk.Dotnet;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// DB Stuff
var connectionString = builder.Configuration.GetConnectionString("DebugConnection");
var serverVersion = ServerVersion.AutoDetect(connectionString);

builder.Services.AddDbContext<CommContext>(options =>
{
    options.UseMySql(connectionString, serverVersion);
});

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
