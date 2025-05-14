namespace TTX.Api.Middleware.options;
public class TtxRateLimitOptions
{
    public int PermitLimit { get; set; } = 20;
    public TimeSpan TimeWindow { get; set; } = TimeSpan.FromSeconds(10);
    public List<string> EndpointPaths { get; set; } = new List<string>();
}

