using System.Security.Claims;
using LogistiCore.Application.Dispatch;
using LogistiCore.Domain.Rules;

namespace LogistiCore.Api.Endpoints;

public static class DispatchEndpoints
{
    private static readonly string[] DispatchRoles = { "Admin", "Dispatcher" };

    public static IEndpointRouteBuilder MapDispatchEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/drivers", async (IDispatchService service, CancellationToken ct) =>
                Results.Ok(await service.ListDriversAsync(ct)))
            .RequireAuthorization(policy => policy.RequireRole(DispatchRoles));

        app.MapPut("/api/v1/drivers/{id:int}/availability", (int id, AvailabilityRequest request, IDispatchService service, CancellationToken ct) =>
                Run(new Dictionary<string, string[]>(), async () => Results.Ok(await service.SetDriverAvailabilityAsync(id, request.IsAvailable, ct))))
            .RequireAuthorization(policy => policy.RequireRole(DispatchRoles));

        app.MapGet("/api/v1/vehicles", async (IDispatchService service, CancellationToken ct) =>
                Results.Ok(await service.ListVehiclesAsync(ct)))
            .RequireAuthorization(policy => policy.RequireRole(DispatchRoles));

        app.MapPost("/api/v1/vehicles", (SaveVehicleRequest request, IDispatchService service, CancellationToken ct) =>
                Run(DispatchValidation.ValidateVehicle(request), async () =>
                {
                    var vehicle = await service.CreateVehicleAsync(request, ct);
                    return Results.Created($"/api/v1/vehicles/{vehicle.VehicleId}", vehicle);
                }))
            .RequireAuthorization(policy => policy.RequireRole(DispatchRoles));

        app.MapPut("/api/v1/vehicles/{id:int}", (int id, SaveVehicleRequest request, IDispatchService service, CancellationToken ct) =>
                Run(DispatchValidation.ValidateVehicle(request), async () => Results.Ok(await service.UpdateVehicleAsync(id, request, ct))))
            .RequireAuthorization(policy => policy.RequireRole(DispatchRoles));

        app.MapGet("/api/v1/dispatch/parcels", async (IDispatchService service, CancellationToken ct) =>
                Results.Ok(await service.ListUnassignedParcelsAsync(ct)))
            .RequireAuthorization(policy => policy.RequireRole(DispatchRoles));

        var routes = app.MapGroup("/api/v1/routes").RequireAuthorization(policy => policy.RequireRole(DispatchRoles));

        routes.MapGet("/", async (DateOnly? date, IDispatchService service, CancellationToken ct) =>
        {
            var day = date ?? DateOnly.FromDateTime(LocalTime.ToLocal(DateTime.UtcNow));
            return Results.Ok(await service.ListRoutesAsync(day, ct));
        });

        routes.MapPost("/", (CreateRouteRequest request, IDispatchService service, CancellationToken ct) =>
            Run(DispatchValidation.ValidateCreateRoute(request), async () =>
            {
                var route = await service.CreateRouteAsync(request, ct);
                return Results.Created($"/api/v1/routes/{route.RouteId}", route);
            }));

        routes.MapPost("/{id:int}/parcels", (int id, AddParcelsRequest request, ClaimsPrincipal user, IDispatchService service, CancellationToken ct) =>
            Run(DispatchValidation.ValidateAddParcels(request), async () =>
                Results.Ok(await service.AddParcelsAsync(id, request, GetUserId(user), ct))));

        routes.MapDelete("/{id:int}/parcels/{parcelId:int}", (int id, int parcelId, ClaimsPrincipal user, IDispatchService service, CancellationToken ct) =>
            Run(new Dictionary<string, string[]>(), async () =>
                Results.Ok(await service.RemoveParcelAsync(id, parcelId, GetUserId(user), ct))));

        routes.MapPost("/{id:int}/release", (int id, IDispatchService service, CancellationToken ct) =>
            Run(new Dictionary<string, string[]>(), async () => Results.Ok(await service.ReleaseRouteAsync(id, ct))));

        routes.MapPost("/{id:int}/cancel", (int id, ClaimsPrincipal user, IDispatchService service, CancellationToken ct) =>
            Run(new Dictionary<string, string[]>(), async () => Results.Ok(await service.CancelRouteAsync(id, GetUserId(user), ct))));

        return app;
    }

    private static async Task<IResult> Run(Dictionary<string, string[]> errors, Func<Task<IResult>> action)
    {
        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }

        try
        {
            return await action();
        }
        catch (BusinessRuleException ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status422UnprocessableEntity, title: "Request not accepted");
        }
    }

    private static int GetUserId(ClaimsPrincipal user) =>
        int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : throw new InvalidOperationException("The signed-in user id is missing from the token.");
}
