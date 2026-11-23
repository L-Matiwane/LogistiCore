using LogistiCore.Application.Parcels;
using LogistiCore.Application.Warehouse;
using Xunit;

namespace LogistiCore.UnitTests;

public class ParcelValidationTests
{
    private static RegisterParcelRequest Valid() => new(
        CustomerId: 1,
        RecipientName: "Anele Dlamini",
        RecipientPhone: "082 123 4567",
        RecipientEmail: null,
        DeliveryAddress: "12 Oxford Street",
        City: "East London",
        PostalCode: "5201",
        DeliveryInstructions: null,
        WeightKg: 1.5m,
        LengthCm: null,
        WidthCm: null,
        HeightCm: null,
        ServiceLevel: "Standard",
        SpecialHandling: null);

    [Fact]
    public void A_complete_request_has_no_errors()
    {
        Assert.Empty(ParcelValidation.ValidateRegister(Valid()));
    }

    [Fact]
    public void Missing_recipient_name_is_reported()
    {
        var errors = ParcelValidation.ValidateRegister(Valid() with { RecipientName = "  " });
        Assert.True(errors.ContainsKey("recipientName"));
    }

    [Fact]
    public void A_phone_number_with_letters_is_rejected()
    {
        var errors = ParcelValidation.ValidateRegister(Valid() with { RecipientPhone = "call me" });
        Assert.True(errors.ContainsKey("recipientPhone"));
    }

    [Fact]
    public void An_email_without_an_at_sign_is_rejected()
    {
        var errors = ParcelValidation.ValidateRegister(Valid() with { RecipientEmail = "not-an-email" });
        Assert.True(errors.ContainsKey("recipientEmail"));
    }

    [Fact]
    public void A_missing_customer_is_reported()
    {
        var errors = ParcelValidation.ValidateRegister(Valid() with { CustomerId = 0 });
        Assert.True(errors.ContainsKey("customerId"));
    }

    [Fact]
    public void Weight_of_zero_is_rejected()
    {
        var errors = ParcelValidation.ValidateRegister(Valid() with { WeightKg = 0m });
        Assert.True(errors.ContainsKey("weightKg"));
    }

    [Fact]
    public void Weight_above_thirty_kilograms_is_rejected()
    {
        var errors = ParcelValidation.ValidateRegister(Valid() with { WeightKg = 31m });
        Assert.True(errors.ContainsKey("weightKg"));
    }

    [Fact]
    public void Exactly_thirty_kilograms_is_accepted()
    {
        Assert.Empty(ParcelValidation.ValidateRegister(Valid() with { WeightKg = 30m }));
    }

    [Fact]
    public void Giving_only_some_dimensions_is_rejected()
    {
        var errors = ParcelValidation.ValidateRegister(Valid() with { LengthCm = 40m });
        Assert.True(errors.ContainsKey("dimensions"));
    }

    [Fact]
    public void All_three_dimensions_together_are_accepted()
    {
        var request = Valid() with { LengthCm = 40m, WidthCm = 35m, HeightCm = 20m };
        Assert.Empty(ParcelValidation.ValidateRegister(request));
    }

    [Fact]
    public void A_dimension_above_300_cm_is_rejected()
    {
        var request = Valid() with { LengthCm = 301m, WidthCm = 35m, HeightCm = 20m };
        Assert.True(ParcelValidation.ValidateRegister(request).ContainsKey("dimensions"));
    }

    [Fact]
    public void An_unknown_service_level_is_rejected()
    {
        var errors = ParcelValidation.ValidateRegister(Valid() with { ServiceLevel = "Turbo" });
        Assert.True(errors.ContainsKey("serviceLevel"));
    }

    [Fact]
    public void An_unknown_handling_option_is_rejected()
    {
        var errors = ParcelValidation.ValidateRegister(Valid() with { SpecialHandling = new[] { "Fragile", "Haunted" } });
        Assert.True(errors.ContainsKey("specialHandling"));
    }

    [Fact]
    public void Quote_validation_checks_weight_and_service_level()
    {
        var errors = ParcelValidation.ValidateQuote(new QuoteRequest(-1m, null, null, null, "Nope"));
        Assert.True(errors.ContainsKey("weightKg"));
        Assert.True(errors.ContainsKey("serviceLevel"));
    }
}

public class WarehouseValidationTests
{
    [Fact]
    public void Scanning_an_empty_tracking_number_is_rejected()
    {
        Assert.True(WarehouseValidation.ValidateScan(new ScanRequest("")).ContainsKey("trackingNo"));
    }

    [Fact]
    public void Scanning_a_tracking_number_is_accepted()
    {
        Assert.Empty(WarehouseValidation.ValidateScan(new ScanRequest("FCL-20261117-000001")));
    }

    [Fact]
    public void Sorting_needs_a_bin()
    {
        var errors = WarehouseValidation.ValidateSort(new SortRequest("FCL-20261117-000001", 0));
        Assert.True(errors.ContainsKey("binId"));
    }

    [Fact]
    public void An_exception_must_be_lost_or_damaged_and_have_a_reason()
    {
        var errors = WarehouseValidation.ValidateException(new ExceptionRequest("FCL-20261117-000001", "Stolen", " "));
        Assert.True(errors.ContainsKey("type"));
        Assert.True(errors.ContainsKey("reason"));
    }

    [Fact]
    public void A_damaged_report_with_a_reason_is_accepted()
    {
        var request = new ExceptionRequest("FCL-20261117-000001", "Damaged", "Box crushed");
        Assert.Empty(WarehouseValidation.ValidateException(request));
    }
}
