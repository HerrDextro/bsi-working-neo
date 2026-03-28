using service.Endpoints;
using service.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();


builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

//http service for calling groq
builder.Services.AddHttpClient<TopicExtractorService>();
builder.Services.AddSingleton<TopicExtractorService>();

//livekit implementation?


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapServiceEndpoints();

app.Run();
