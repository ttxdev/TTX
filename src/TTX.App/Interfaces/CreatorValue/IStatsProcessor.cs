using TTX.App.Repositories.CreatorValue;
using TTX.Domain.Models;

namespace TTX.App.Interfaces.CreatorValue;

public interface IStatsProcessor
{
    Task<double> Process(Creator creator, CreatorStats? stats = null);
}
