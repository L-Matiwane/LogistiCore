using System.Security.Claims;
using LogistiCore.Application.Warehouse;
using LogistiCore.Domain.Rules;

namespace LogistiCore.Api.Endpoints;

public static class WarehouseEndpoints
{
    private static readonly string[] StaffRoles = { "Admin", "WarehouseClerk", "Dispatcher", "Billing" };
    private static readonly string[] WarehouseRoles = { "Admin", "WarehouseClerk" };

    public static IEndpointRouteBuilder MapWarehouseEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/warehouse");

        group.MapGet("/bins", async (IWarehouseService service, CancellationToken ct) =>
                Results.Ok(await service.ListBinsAsync(ct)))
            .RequireAuthorization(policy => policy.RequireRole(StaffRoles));

        group.MapGet("/inventory", async (int? binId, IWarehouseService service, CancellationToken ct) =>
                Results.Ok(await service.ListInventoryAsync(binId, ct)))
            .RequireAuthorization(policy => policy.RequireRole(StaffRoles));

        group.MapGet("/exceptions", async (IWarehouseService service, CancellationToken ct) =>
                Results.Ok(await service.ListExceptionsAsync(ct)))
            .RequireAuthorization(policy => policy.RequireRole(StaffRoles));

        group.MapPost("/receive", (ScanRequest request, ClaimsPrincipal user, IWarehouseService service, CancellationToken ct) =>
                Run(WarehouseValidation.ValidateScan(request), async () => Results.Ok(await service.ReceiveAsync(request, GetUserId(user), ct))))
            .RequireAuthorization(policy => policy.RequireRole(WarehouseRoles));

        group.MapPost("/sort", (SortRequest request, ClaimsPrincipal user, IWarehouseService service, CancellationToken ct) =>
                Run(WarehouseValidation.ValidateSort(request), async () => Results.Ok(await service.SortAsync(request, GetUserId(user), ct))))
            .RequireAuthorization(policy => policy.RequireRole(WarehouseRoles));

        group.MapPost("/exceptions", (ExceptionRequest request, ClaimsPrincipal user, IWarehouseService service, CancellationToken ct) =>
                Run(WarehouseValidation.ValidateException(request), async () => Results.Ok(await service.ReportExceptionAsync(request, GetUserId(user), ct))))
            .RequireAuthorization(policy => policy.RequireRole(WarehouseRoles));

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
