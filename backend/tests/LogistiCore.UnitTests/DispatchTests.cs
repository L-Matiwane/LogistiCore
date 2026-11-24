using LogistiCore.Application.Dispatch;
using LogistiCore.Domain.Rules;
using Xunit;

namespace LogistiCore.UnitTests;

public class RouteRulesTests
{
    [Fact]
    public void Parcels_can_only_be_edited_while_a_route_is_planned()
    {
        Assert.True(RouteRules.CanEdit(RouteStatus.Planned));
        Assert.False(RouteRules.CanEdit(RouteStatus.Released));
        Assert.False(RouteRules.CanEdit(RouteStatus.InProgress));
        Assert.False(RouteRules.CanEdit(RouteStatus.Completed));
        Assert.False(RouteRules.CanEdit(RouteStatus.Cancelled));
    }

    [Fact]
    public void Only_a_planned_route_can_be_released()
    {
        Assert.True(RouteRules.CanRelease(RouteStatus.Planned));
        Assert.False(RouteRules.CanRelease(RouteStatus.Released));
        Assert.False(RouteRules.CanRelease(RouteStatus.Cancelled));
    }

    [Fact]
    public void Planned_and_released_routes_can_be_cancelled_but_started_or_finished_routes_cannot()
    {
        Assert.True(RouteRules.CanCancel(RouteStatus.Planned));
        Assert.True(RouteRules.CanCancel(RouteStatus.Released));
        Assert.False(RouteRules.CanCancel(RouteStatus.InProgress));
        Assert.False(RouteRules.CanCancel(RouteStatus.Completed));
        Assert.False(RouteRules.CanCancel(RouteStatus.Cancelled));
    }

    [Fact]
    public void A_load_up_to_the_capacity_fits()
    {
        Assert.True(RouteRules.FitsInVehicle(100m, 50m, 150m));
    }

    [Fact]
    public void A_load_over_the_capacity_does_not_fit()
    {
        Assert.False(RouteRules.FitsInVehicle(100m, 50.01m, 150m));
    }

    [Fact]
    public void An_empty_route_accepts_a_parcel_that_matches_the_capacity_exactly()
    {
        Assert.True(RouteRules.FitsInVehicle(0m, 30m, 30m));
    }
}

public class DispatchValidationTests
{
    private static DateOnly Today() => DateOnly.FromDateTime(LocalTime.ToLocal(DateTime.UtcNow));

    [Fact]
    public void A_route_for_today_with_a_name_driver_and_vehicle_is_valid()
    {
        var errors = DispatchValidation.ValidateCreateRoute(new CreateRouteRequest("Route A", Today(), 1, 1));
        Assert.Empty(errors);
    }

    [Fact]
    public void A_route_needs_a_name()
    {
        var errors = DispatchValidation.ValidateCreateRoute(new CreateRouteRequest(" ", Today(), 1, 1));
        Assert.True(errors.ContainsKey("name"));
    }

    [Fact]
    public void A_route_cannot_be_planned_for_a_past_day()
    {
        var errors = DispatchValidation.ValidateCreateRoute(new CreateRouteRequest("Route A", Today().AddDays(-1), 1, 1));
        Assert.True(errors.ContainsKey("routeDate"));
    }

    [Fact]
    public void A_route_needs_a_driver_and_a_vehicle()
    {
        var errors = DispatchValidation.ValidateCreateRoute(new CreateRouteRequest("Route A", Today(), 0, 0));
        Assert.True(errors.ContainsKey("driverId"));
        Assert.True(errors.ContainsKey("vehicleId"));
    }

    [Fact]
    public void Adding_parcels_needs_at_least_one_parcel()
    {
        Assert.True(DispatchValidation.ValidateAddParcels(new AddParcelsRequest(null)).ContainsKey("parcelIds"));
        Assert.True(DispatchValidation.ValidateAddParcels(new AddParcelsRequest(Array.Empty<int>())).ContainsKey("parcelIds"));
    }

    [Fact]
    public void Adding_parcels_rejects_invalid_ids()
    {
        Assert.True(DispatchValidation.ValidateAddParcels(new AddParcelsRequest(new[] { 3, 0 })).ContainsKey("parcelIds"));
    }

    [Fact]
    public void Adding_a_list_of_valid_parcels_is_accepted()
    {
        Assert.Empty(DispatchValidation.ValidateAddParcels(new AddParcelsRequest(new[] { 1, 2, 3 })));
    }

    [Fact]
    public void A_vehicle_needs_a_registration_and_a_sensible_capacity()
    {
        var errors = DispatchValidation.ValidateVehicle(new SaveVehicleRequest("", null, 0m, true));
        Assert.True(errors.ContainsKey("registration"));
        Assert.True(errors.ContainsKey("capacityKg"));
    }

    [Fact]
    public void A_complete_vehicle_is_accepted()
    {
        Assert.Empty(DispatchValidation.ValidateVehicle(new SaveVehicleRequest("EL 789 EC", "Small van", 450m, true)));
    }

    [Fact]
    public void A_vehicle_capacity_above_20000_kg_is_rejected()
    {
        var errors = DispatchValidation.ValidateVehicle(new SaveVehicleRequest("EL 789 EC", null, 20001m, true));
        Assert.True(errors.ContainsKey("capacityKg"));
    }
}
