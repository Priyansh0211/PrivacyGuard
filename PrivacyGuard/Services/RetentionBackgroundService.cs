using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PrivacyGuard.Interfaces.Services;

namespace PrivacyGuard.Services
{
    public class RetentionBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RetentionBackgroundService> _logger;
        private static readonly TimeSpan RunInterval = TimeSpan.FromHours(24);
        private static readonly TimeSpan InitialDelay = TimeSpan.FromMinutes(1);

        public RetentionBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<RetentionBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("RetentionBackgroundService started.");

            try
            {
                await Task.Delay(InitialDelay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                await RunJobCycleAsync(stoppingToken);

                try
                {
                    await Task.Delay(RunInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            _logger.LogInformation("RetentionBackgroundService stopped.");
        }

        private async Task RunJobCycleAsync(CancellationToken stoppingToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var retentionService = scope.ServiceProvider.GetRequiredService<IRetentionPolicyService>();

                var processed = await retentionService.ExecuteDailyErasureJobAsync();
                _logger.LogInformation("Daily retention job completed. Processed records: {Count}", processed);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Shutting down gracefully
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error occurred in RetentionBackgroundService cycle.");
            }
        }
    }
}