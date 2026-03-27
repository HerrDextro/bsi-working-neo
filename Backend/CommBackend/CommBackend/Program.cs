using CommBackend.Endpoints;
using Livekit.Server.Sdk.Dotnet;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

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

apiSecret = apiSecret.PadRight(32, '\0'); //pad over because the key is too short :(

builder.Services.AddSingleton(new RoomServiceClient(host, apiKey, apiSecret));

var app = builder.Build();

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
