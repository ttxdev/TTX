using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TTX.Commands.Players.CalculatePortfolio;
using TTX.Infrastructure.Data;

namespace TTX.PortfolioMonitor.Services;

public class PortfolioMonitorService(ApplicationDbContext context, IServiceProvider services)
{
    public async Task Start(int buffer, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            await Calculate(ct);
            await Task.Delay(buffer, ct);
        }
    }

    private async Task Calculate(CancellationToken ct)
    {
        await foreach (var player in context.Players.AsAsyncEnumerable().WithCancellation(ct))
        {
            await using var scope = services.CreateAsyncScope();
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            await sender.Send(new CalculatePortfolioCommand
            {
                PlayerId = player.Id
            }, ct);
        }
    }
}