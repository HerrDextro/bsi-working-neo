using CommBackend.Models.Context;
using CommBackend.Models.Data;
using CommBackend.Models.Presentation.Auth;
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

            app.MapGet("/teams", async (CommContext db, bool activeOnly) =>
            {
                IQueryable<TeamsCall> calls = db.TeamsCalls;
                DateTime now = DateTime.Now;
                if (activeOnly)
                {
                    calls.Where(c => c.StartTime < now && now < c.EndTime);
                }

                List<TeamsCall> teamsCalls = calls.ToList();

                List<TeamsResponse> Users = calls.Select(call => new TeamsResponse(call)).ToList();

                return Users;
            });
        }
    }
}
