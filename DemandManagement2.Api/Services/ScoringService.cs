using DemandManagement2.Domain.Entities;

namespace DemandManagement2.Api.Services;

public static class ScoringService
{
    // Returns 0..100 (higher = better)
    public static decimal CalculateScore(DemandRequest demand, Assessment assessment)
    {
        // Inputs are 1..5
        // Convert "bad when high" factors (Cost, Risk, ResourceNeed) into "good when high"
        int costGood = 6 - assessment.CostImpact;
        int riskGood = 6 - assessment.Risk;
        int resourceGood = 6 - assessment.ResourceNeed;

        // Weights (adjust to your governance model)
        decimal score =
            (assessment.BusinessValue * 0.30m) +
            (assessment.StrategicAlignment * 0.25m) +
            (demand.Urgency * 0.15m) +
            (costGood * 0.10m) +
            (riskGood * 0.10m) +
            (resourceGood * 0.10m);

        // score currently in range 1..5 scale; convert to 0..100
        return Math.Round((score / 5m) * 100m, 2);
    }
}