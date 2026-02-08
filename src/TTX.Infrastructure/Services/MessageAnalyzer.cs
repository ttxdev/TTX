using TTX.App.Interfaces.CreatorValue;

namespace TTX.Infrastructure.Services;

public class MessageAnalyzer : IMessageAnalyzer
{
    public Task<double> Analyze(string content)
    {
        if (content.Contains("+2"))
        {
            return Task.FromResult(2.0);
        }
        if (content.Contains("-2"))
        {
            return Task.FromResult(-2.0);
        }

        return Task.FromResult(0.0);
    }
}
