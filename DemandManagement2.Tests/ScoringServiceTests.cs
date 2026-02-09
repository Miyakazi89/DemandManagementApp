using DemandManagement2.Api.Services;
using DemandManagement2.Domain.Entities;

namespace DemandManagement2.Tests;

public class ScoringServiceTests
{
    private static DemandRequest MakeDemand(int urgency = 3) => new()
    {
        Title = "Test",
        Urgency = urgency
    };

    private static Assessment MakeAssessment(
        int bv = 3, int cost = 3, int risk = 3, int resource = 3, int alignment = 3) => new()
    {
        BusinessValue = bv,
        CostImpact = cost,
        Risk = risk,
        ResourceNeed = resource,
        StrategicAlignment = alignment
    };

    [Fact]
    public void AllMiddleScores_ReturnsMiddleResult()
    {
        var demand = MakeDemand(urgency: 3);
        var assessment = MakeAssessment(3, 3, 3, 3, 3);

        var score = ScoringService.CalculateScore(demand, assessment);

        Assert.Equal(60m, score);
    }

    [Fact]
    public void AllMaxScores_Returns100()
    {
        // Best case: high BV, high alignment, high urgency, LOW cost/risk/resource (1)
        var demand = MakeDemand(urgency: 5);
        var assessment = MakeAssessment(bv: 5, cost: 1, risk: 1, resource: 1, alignment: 5);

        var score = ScoringService.CalculateScore(demand, assessment);

        Assert.Equal(100m, score);
    }

    [Fact]
    public void AllMinScores_Returns20()
    {
        // Worst case: low BV, low alignment, low urgency, HIGH cost/risk/resource (5)
        var demand = MakeDemand(urgency: 1);
        var assessment = MakeAssessment(bv: 1, cost: 5, risk: 5, resource: 5, alignment: 1);

        var score = ScoringService.CalculateScore(demand, assessment);

        Assert.Equal(20m, score);
    }

    [Fact]
    public void HighBusinessValue_IncreasesScore()
    {
        var demand = MakeDemand(urgency: 3);
        var low = MakeAssessment(bv: 1);
        var high = MakeAssessment(bv: 5);

        var scoreLow = ScoringService.CalculateScore(demand, low);
        var scoreHigh = ScoringService.CalculateScore(demand, high);

        Assert.True(scoreHigh > scoreLow);
    }

    [Fact]
    public void HighCost_DecreasesScore()
    {
        // Cost is inverted: higher cost = lower score
        var demand = MakeDemand(urgency: 3);
        var cheap = MakeAssessment(cost: 1);
        var expensive = MakeAssessment(cost: 5);

        var scoreCheap = ScoringService.CalculateScore(demand, cheap);
        var scoreExpensive = ScoringService.CalculateScore(demand, expensive);

        Assert.True(scoreCheap > scoreExpensive);
    }

    [Fact]
    public void HighRisk_DecreasesScore()
    {
        var demand = MakeDemand(urgency: 3);
        var lowRisk = MakeAssessment(risk: 1);
        var highRisk = MakeAssessment(risk: 5);

        var scoreLow = ScoringService.CalculateScore(demand, lowRisk);
        var scoreHigh = ScoringService.CalculateScore(demand, highRisk);

        Assert.True(scoreLow > scoreHigh);
    }

    [Fact]
    public void ScoreIsAlwaysBetween0And100()
    {
        // Test many combinations
        for (int bv = 1; bv <= 5; bv++)
        for (int urg = 1; urg <= 5; urg++)
        {
            var demand = MakeDemand(urgency: urg);
            var assessment = MakeAssessment(bv: bv, cost: 3, risk: 3, resource: 3, alignment: 3);

            var score = ScoringService.CalculateScore(demand, assessment);

            Assert.InRange(score, 0m, 100m);
        }
    }
}
