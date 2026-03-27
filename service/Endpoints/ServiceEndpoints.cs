using service.Dtos;
using service.Services;

namespace service.Endpoints
{
    public static class ServiceEndpoints
    {
        public static void MapServiceEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/health", () => Results.Ok("Service is healthy"));
            // Future endpoints can be added here, e.g.:
            // app.MapPost("/extract-topic", async (TopicExtractorService topicExtractor, string transcript) =>
            // {
            //     var topic = await topicExtractor.GetTopicFromTranscript(transcript);
            //     return Results.Ok(topic);
            // });

            app.MapPost("/rooms/{roomId}/update-topic", async (string roomId, RoomTopicDto dto, TopicExtractorService extractor) =>
            {
                bool isBranch = false;
                // 1. Get the smart topic from Groq
                var newTopic = await extractor.GetTopicFromTranscript(roomId, dto.Text);

                // 2. TODO: Update LiveKit Room Metadata here using the RoomServiceClient
                // For now, just return it to prove it works!
                return Results.Ok(
                    new
                    {
                        Room = roomId,
                        Topic = newTopic.Topic,
                        isBranch = newTopic.IsBranch,
                        debug_llm_said = newTopic.RawResponse // See the "Unsanitized" truth});
                    });
            });
        }
    }
}
