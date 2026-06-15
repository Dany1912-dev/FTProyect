using tusdotnet.Stores;

namespace FtpCloud.Api.Services;

public class TusCleanupService(TusDiskStore store, ILogger<TusCleanupService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await store.RemoveExpiredFilesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error limpiando uploads TUS expirados");
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}
