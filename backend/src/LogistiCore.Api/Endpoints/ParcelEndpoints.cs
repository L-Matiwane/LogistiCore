using System.Security.Claims;
using LogistiCore.Application.Customers;
using LogistiCore.Application.Dashboard;
using LogistiCore.Application.Parcels;
using LogistiCore.Domain.Rules;

namespace LogistiCore.Api.Endpoints;

public static class ParcelEndpoints
{
    private static readonly string[] StaffRoles = { "Admin", "WarehouseClerk", "Dispatcher", "Billing" };
    private static readonly string[] IntakeRoles = { "Admin", "WarehouseClerk" };

    public static IEndpointRouteBuilder MapParcelEndpoints(this IEndpointRouteBuilder app)
    {
        var parcels = app.MapGroup("/api/v1/parcels");

        parcels.MapPost("/quote", async (QuoteRequest request, IParcelService service, CancellationToken ct) =>
        {
            var errors = ParcelValidation.ValidateQuote(request);
            if (errors.Count > 0)
            {
                return Results.ValidationProblem(errors);
            }

            try
            {
                return Results.Ok(await service.QuoteAsync(request, ct));
            }
            catch (BusinessRuleException ex)
            {
                return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status422UnprocessableEntity, title: "No price available");
            }
        }).RequireAuthorization(policy => policy.RequireRole(IntakeRoles));

        parcels.MapPost("/", async (RegisterParcelRequest request, ClaimsPrincipal user, IParcelService service, CancellationToken ct) =>
        {
            var errors = ParcelValidation.ValidateRegister(request);
            if (errors.Count > 0)
            {
                return Results.ValidationProblem(errors);
            }

            try
            {
                var parcel = await service.RegisterAsync(request, GetUserId(user), ct);
                return Results.Created($"/api/v1/parcels/{parcel.ParcelId}", parcel);
            }
            catch (BusinessRuleException ex)
            {
                return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status422UnprocessableEntity, title: "Parcel not registered");
            }
        }).RequireAuthorization(policy => policy.RequireRole(IntakeRoles));

        parcels.MapGet("/", async (
            string? q,
            string? status,
            int? customerId,
            DateOnly? from,
            DateOnly? to,
            IParcelService service,
            CancellationToken ct,
            int page = 1,
            int pageSize = 20) =>
        {
            var result = await service.SearchAsync(new ParcelSearch(q, status, customerId, from, to, page, pageSize), ct);
            return Results.Ok(result);
        }).RequireAuthorization(policy => policy.RequireRole(StaffRoles));

        parcels.MapGet("/{id:int}", async (int id, IParcelService service, CancellationToken ct) =>
        {
            var parcel = await service.GetAsync(id, ct);
            return parcel is null ? Results.NotFound() : Results.Ok(parcel);
        }).RequireAuthorization(policy => policy.RequireRole(StaffRoles));

        app.MapGet("/api/v1/customers", async (ICustomerLookup lookup, CancellationToken ct) =>
                Results.Ok(await lookup.ListActiveAsync(ct)))
            .RequireAuthorization(policy => policy.RequireRole(StaffRoles));

        app.MapGet("/api/v1/dashboard/summary", async (IDashboardService dashboard, CancellationToken ct) =>
                Results.Ok(await dashboard.GetSummaryAsync(ct)))
            .RequireAuthorization(policy => policy.RequireRole(StaffRoles));

        return app;
    }

    private static int GetUserId(ClaimsPrincipal user) =>
        int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : throw new InvalidOperationException("The signed-in user id is missing from the token.");
}
