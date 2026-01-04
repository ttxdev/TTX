using TTX.App.Dto.Portfolio;

namespace TTX.Infrastructure.Data.Repositories;

internal static class TimeStepExtension
{
    public static string ToTimescaleString(this TimeStep step) =>
        step switch
        {
            TimeStep.Minute => "1 minute",
            TimeStep.FiveMinute => "5 minute",
            TimeStep.FifteenMinute => "15 minute",
            TimeStep.ThirtyMinute => "30 minute",
            TimeStep.Hour => "1 hour",
            TimeStep.Day => "1 day",
            TimeStep.Week => "1 week",
            TimeStep.Month => "1 month",
            _ => throw new NotImplementedException()
        };
}
