namespace TTX.App.Interfaces.CreatorValue;

public interface IMessageAnalyzer
{
    public Task<double> Analyze(string content);
}
