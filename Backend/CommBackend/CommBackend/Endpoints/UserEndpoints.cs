namespace CommBackend.Endpoints
{
    public static class UserEndpoints
    {
        public static void MapUserEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/Auth", () =>
            {
                return Results.Ok("Test");
            });
        }
    }
}
