usingpublic class CustomFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove real TopicExtractorService
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(TopicExtractorService));

            if (descriptor != null)
                services.Remove(descriptor);

            // Register mock
            var mockService = new Mock<TopicExtractorService>(
                new HttpClient(new MockHttpMessageHandler(
                    new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent("""
                        {
                            "choices":[
                                { "message": { "content": "test topic" } }
                            ]
                        }
                        """)
                    })),
                new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        { "GroqConfig:GROQ_API_KEY", "test" }
                    })
                    .Build()
            );

            services.AddSingleton(mockService.Object);
        });
    }
}