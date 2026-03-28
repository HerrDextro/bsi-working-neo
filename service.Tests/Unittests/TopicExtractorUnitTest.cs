using System.Net;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using service.Services;
using Xunit;

namespace service.Tests;

public class TopicExtractorServiceTests
{
    private IConfiguration GetConfig()
    {
        var dict = new Dictionary<string, string>
        {
            { "GroqConfig:GROQ_API_KEY", "test-key" }
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(dict)
            .Build();
    }

    private HttpClient CreateHttpClient(string content)
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(content)
        };

        var handler = new MockHttpMessageHandler(response);
        return new HttpClient(handler);
    }

    [Fact]
    public async Task Returns_Clean_Topic_When_No_Branch()
    {
        var fakeJson = """
        {
            "choices": [
                {
                    "message": {
                        "content": "sports discussion"
                    }
                }
            ]
        }
        """;

        var client = CreateHttpClient(fakeJson);
        var config = GetConfig();

        var service = new TopicExtractorService(client, config);

        var result = await service.GetTopicFromTranscript("room1", "talking about football");

        Assert.Equal("sports discussion", result.Topic);
        Assert.False(result.IsBranch);
        Assert.Equal("sports discussion", result.RawResponse);
    }

    [Fact]
    public async Task Detects_Branch_When_Tag_Present()
    {
        var fakeJson = """
        {
            "choices": [
                {
                    "message": {
                        "content": "[BRANCH] new topic shift"
                    }
                }
            ]
        }
        """;

        var client = CreateHttpClient(fakeJson);
        var config = GetConfig();

        var service = new TopicExtractorService(client, config);

        var result = await service.GetTopicFromTranscript("room2", "completely different topic");

        Assert.Equal("new topic shift", result.Topic);
        Assert.True(result.IsBranch);
    }

    [Fact]
    public async Task Falls_Back_On_Exception()
    {
        var handler = new MockHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.InternalServerError)
        );

        var client = new HttpClient(handler);
        var config = GetConfig();

        var service = new TopicExtractorService(client, config);

        var result = await service.GetTopicFromTranscript("room3", "input");

        Assert.Equal("General Chat", result.Topic);
        Assert.False(result.IsBranch);
    }
}