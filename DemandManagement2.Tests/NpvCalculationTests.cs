using System.Reflection;
using DemandManagement2.Api.Controllers;

namespace DemandManagement2.Tests;

public class NpvCalculationTests
{
    // Access the private static CalculateNpv method via reflection
    private static decimal CalculateNpv(decimal initialCost, decimal annualBenefit, int years, decimal discountRate)
    {
        var method = typeof(AssessmentsController)
            .GetMethod("CalculateNpv", BindingFlags.NonPublic | BindingFlags.Static);

        return (decimal)method!.Invoke(null, new object[] { initialCost, annualBenefit, years, discountRate })!;
    }

    [Fact]
    public void ZeroYears_ReturnsNegativeInitialCost()
    {
        var npv = CalculateNpv(100_000m, 50_000m, 0, 10m);

        Assert.Equal(-100_000m, npv);
    }

    [Fact]
    public void OneYear_CalculatesCorrectly()
    {
        // NPV = -100,000 + 50,000 / (1.10) = -100,000 + 45,454.55 = -54,545.45
        var npv = CalculateNpv(100_000m, 50_000m, 1, 10m);

        Assert.Equal(-54_545.45m, npv);
    }

    [Fact]
    public void ZeroDiscountRate_SumsDirectly()
    {
        // NPV = -100,000 + (50,000 * 5) = 150,000
        var npv = CalculateNpv(100_000m, 50_000m, 5, 0m);

        Assert.Equal(150_000m, npv);
    }

    [Fact]
    public void BreakEvenScenario_NpvNearZero()
    {
        // With 0% discount rate, 2 years of 50k benefit matches 100k cost
        var npv = CalculateNpv(100_000m, 50_000m, 2, 0m);

        Assert.Equal(0m, npv);
    }

    [Fact]
    public void HighDiscount_ReducesFutureValue()
    {
        var npvLow = CalculateNpv(100_000m, 50_000m, 5, 5m);
        var npvHigh = CalculateNpv(100_000m, 50_000m, 5, 20m);

        Assert.True(npvLow > npvHigh);
    }

    [Fact]
    public void MoreYears_IncreasesNpv()
    {
        var npvShort = CalculateNpv(100_000m, 50_000m, 2, 10m);
        var npvLong = CalculateNpv(100_000m, 50_000m, 10, 10m);

        Assert.True(npvLong > npvShort);
    }

    [Fact]
    public void ZeroCostZeroBenefit_ReturnsZero()
    {
        var npv = CalculateNpv(0m, 0m, 5, 10m);

        Assert.Equal(0m, npv);
    }

    [Fact]
    public void ResultIsRoundedToTwoDecimals()
    {
        var npv = CalculateNpv(100_000m, 33_333m, 3, 7m);

        // Verify it has at most 2 decimal places
        var rounded = Math.Round(npv, 2);
        Assert.Equal(rounded, npv);
    }
}
