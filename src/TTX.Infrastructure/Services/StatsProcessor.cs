using TTX.App.Interfaces.CreatorValue;
using TTX.App.Interfaces.Data.CreatorValue;
using TTX.Domain.Models;

namespace TTX.Infrastructure.Services;

public class StatsProcessor : IStatsProcessor
{
    public Task<double> Process(Creator creator, CreatorStats? stats = null)
    {
        return Task.FromResult(stats is not null
            ? stats.Positive + stats.Negative
            : 0.0);
    }
}
