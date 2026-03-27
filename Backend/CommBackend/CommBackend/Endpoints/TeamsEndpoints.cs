using CommBackend.Models.Context;
using CommBackend.Models.Data;
using CommBackend.Models.Presentation.Teams;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CommBackend.Endpoints
{
    public static class TeamsEndpoints
    {
        public static void MapTeamsEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/teams",(CommContext db, TeamsRequest teamsRequest) =>
            {
                TeamsCall teamsCall = teamsRequest.AsTeamsCall();

                db.Add(teamsCall);

                return Results.Ok(new TeamsResponse(teamsCall));
            });

            //app.MapGet("/teams", ())
        }
    }
}
