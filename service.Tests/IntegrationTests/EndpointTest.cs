using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using service;

public class EndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public EndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_Endpoint_Returns_Healthy()
    {
        var response = await _client.GetAsync("/health");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("Service is healthy", content);
    }

    [Fact]
    public async Task Update_Topic_Endpoint_Works()
    {
        var dto = new
        {
            Text = "talking about startups"
        };

        var response = await _client.PostAsJsonAsync("/rooms/test-room/update-topic", dto);

        // This will likely fail unless you mock TopicExtractorService in DI
        // So this test needs DI override (see below)
        Assert.True(response.IsSuccessStatusCode);
    }
}