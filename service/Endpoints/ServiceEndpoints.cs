using service.Dtos;
using service.Services;

namespace service.Endpoints
{
    public static class ServiceEndpoints
    {
        public static void MapServiceEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/health", () => Results.Ok("Service is healthy"));

            app.MapPost("/rooms/{roomId}/update-topic", async (string roomId, RoomTopicDto dto, TopicExtractorService extractor) =>
            {
                bool isBranch = false;
                var newTopic = await extractor.GetTopicFromTranscript(roomId, dto.Text);

                return Results.Ok(
                    new
                    {
                        Room = roomId,
                        Topic = newTopic.Topic,
                        isBranch = newTopic.IsBranch,
                        debug_llm_said = newTopic.RawResponse 
                    });
            });
        }
    }
}
