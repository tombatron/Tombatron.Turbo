using Microsoft.AspNetCore.SignalR;
using Tombatron.Turbo.Streams;

namespace Tombatron.Turbo.Sample;

public class StreamBackgroundService(IHubContext<TurboHub> turboHub) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var html = $"<div id=\"clock\">{DateTime.Now:HH:mm:ss}</span>";

            var timeUpdate = new TurboStreamBuilder()
                .Replace("clock", html)
                .Build();

            await turboHub.Clients.All.SendAsync(TurboHub.TurboStreamMethod, timeUpdate, stoppingToken);

            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }
}
