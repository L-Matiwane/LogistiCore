using LogistiCore.Domain.Rules;
using Xunit;

namespace LogistiCore.UnitTests;

public class PricingRulesTests
{
    [Fact]
    public void Volumetric_weight_is_volume_divided_by_5000()
    {
        // 40 x 35 x 20 = 28 000 cubic cm, divided by 5000 = 5.6 kg
        Assert.Equal(5.60m, PricingRules.VolumetricWeightKg(40m, 35m, 20m));
    }

    [Fact]
    public void Volumetric_weight_is_rounded_to_two_decimals()
    {
        // 33 x 33 x 33 = 35 937, divided by 5000 = 7.1874, rounds to 7.19
        Assert.Equal(7.19m, PricingRules.VolumetricWeightKg(33m, 33m, 33m));
    }

    [Fact]
    public void Volumetric_weight_is_zero_when_a_dimension_is_missing()
    {
        Assert.Equal(0m, PricingRules.VolumetricWeightKg(null, 35m, 20m));
        Assert.Equal(0m, PricingRules.VolumetricWeightKg(40m, null, 20m));
        Assert.Equal(0m, PricingRules.VolumetricWeightKg(40m, 35m, null));
    }

    [Fact]
    public void Volumetric_weight_is_zero_when_a_dimension_is_zero_or_negative()
    {
        Assert.Equal(0m, PricingRules.VolumetricWeightKg(0m, 35m, 20m));
        Assert.Equal(0m, PricingRules.VolumetricWeightKg(40m, -1m, 20m));
    }

    [Fact]
    public void Billed_weight_is_the_larger_of_actual_and_volumetric()
    {
        Assert.Equal(5.6m, PricingRules.BilledWeightKg(4.2m, 5.6m));
        Assert.Equal(8m, PricingRules.BilledWeightKg(8m, 5.6m));
    }

    [Fact]
    public void Billed_weight_equals_actual_weight_when_there_is_no_volumetric_weight()
    {
        Assert.Equal(1.5m, PricingRules.BilledWeightKg(1.5m, 0m));
    }
}
