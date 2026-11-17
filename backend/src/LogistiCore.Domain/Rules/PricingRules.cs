namespace LogistiCore.Domain.Rules;

public static class PricingRules
{
    /// <summary>Courier industry standard: 5000 cubic centimetres per kilogram.</summary>
    public const decimal VolumetricDivisor = 5000m;

    /// <summary>Heaviest parcel the rate cards cover.</summary>
    public const decimal MaxWeightKg = 30m;

    public static decimal VolumetricWeightKg(decimal? lengthCm, decimal? widthCm, decimal? heightCm)
    {
        if (lengthCm is not > 0 || widthCm is not > 0 || heightCm is not > 0)
        {
            return 0m;
        }

        var volume = lengthCm.Value * widthCm.Value * heightCm.Value;
        return Math.Round(volume / VolumetricDivisor, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>The courier bills whichever is larger: actual weight or volumetric weight.</summary>
    public static decimal BilledWeightKg(decimal actualKg, decimal volumetricKg) =>
        Math.Max(actualKg, volumetricKg);
}
