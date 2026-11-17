using System.Text.RegularExpressions;
using LogistiCore.Domain.Rules;

namespace LogistiCore.Application.Parcels;

public static class ParcelValidation
{
    private static readonly Regex PhonePattern = new(@"^[0-9+()\-\s]{9,20}$");
    private static readonly Regex EmailPattern = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

    public static Dictionary<string, string[]> ValidateRegister(RegisterParcelRequest r)
    {
        var errors = new Dictionary<string, string[]>();

        if (r.CustomerId <= 0)
        {
            errors["customerId"] = new[] { "Choose a customer account." };
        }

        Required(errors, "recipientName", r.RecipientName, 150, "Recipient name");
        Required(errors, "deliveryAddress", r.DeliveryAddress, 300, "Street address");
        Required(errors, "city", r.City, 100, "City");
        Required(errors, "postalCode", r.PostalCode, 10, "Postal code");

        if (string.IsNullOrWhiteSpace(r.RecipientPhone))
        {
            errors["recipientPhone"] = new[] { "Phone number is required." };
        }
        else if (!PhonePattern.IsMatch(r.RecipientPhone.Trim()))
        {
            errors["recipientPhone"] = new[] { "Enter a valid phone number, for example 082 123 4567." };
        }

        if (!string.IsNullOrWhiteSpace(r.RecipientEmail) &&
            (r.RecipientEmail.Trim().Length > 256 || !EmailPattern.IsMatch(r.RecipientEmail.Trim())))
        {
            errors["recipientEmail"] = new[] { "Enter a valid email address." };
        }

        if (r.DeliveryInstructions is { Length: > 300 })
        {
            errors["deliveryInstructions"] = new[] { "Instructions must be 300 characters or fewer." };
        }

        if (r.SpecialHandling is not null && r.SpecialHandling.Any(t => !HandlingTags.All.Contains(t)))
        {
            errors["specialHandling"] = new[] { "One of the handling options is not recognised." };
        }

        ValidateSpecs(errors, r.WeightKg, r.LengthCm, r.WidthCm, r.HeightCm, r.ServiceLevel);
        return errors;
    }

    public static Dictionary<string, string[]> ValidateQuote(QuoteRequest q)
    {
        var errors = new Dictionary<string, string[]>();
        ValidateSpecs(errors, q.WeightKg, q.LengthCm, q.WidthCm, q.HeightCm, q.ServiceLevel);
        return errors;
    }

    private static void Required(Dictionary<string, string[]> errors, string field, string? value, int max, string label)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors[field] = new[] { $"{label} is required." };
        }
        else if (value.Trim().Length > max)
        {
            errors[field] = new[] { $"{label} must be {max} characters or fewer." };
        }
    }

    private static void ValidateSpecs(
        Dictionary<string, string[]> errors,
        decimal weightKg,
        decimal? lengthCm,
        decimal? widthCm,
        decimal? heightCm,
        string serviceLevel)
    {
        if (weightKg <= 0)
        {
            errors["weightKg"] = new[] { "Weight must be greater than 0 kg." };
        }
        else if (weightKg > PricingRules.MaxWeightKg)
        {
            errors["weightKg"] = new[] { $"Weight above {PricingRules.MaxWeightKg:0} kg is not supported yet." };
        }

        var dims = new[] { lengthCm, widthCm, heightCm };
        var given = dims.Count(d => d.HasValue);

        if (given is > 0 and < 3)
        {
            errors["dimensions"] = new[] { "Enter all three dimensions, or leave them all blank." };
        }
        else if (given == 3 && dims.Any(d => d <= 0 || d > 300))
        {
            errors["dimensions"] = new[] { "Each dimension must be between 1 and 300 cm." };
        }

        if (!ServiceLevels.All.Contains(serviceLevel))
        {
            errors["serviceLevel"] = new[] { "Choose Standard, Express or Overnight." };
        }
    }
}
