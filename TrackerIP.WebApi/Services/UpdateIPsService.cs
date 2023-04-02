namespace TrackerIP.WebApi.Services
{
    public class UpdateIPsService : BackgroundService
    {
        private readonly ILogger<UpdateIPsService> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public UpdateIPsService(IServiceScopeFactory serviceScopeFactory, ILogger<UpdateIPsService> logger) 
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Update IPs background process started");

                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var trackerIPService = scope.ServiceProvider.GetRequiredService<ITrackerIPService>();
                        await trackerIPService.UpdateIPDetailsAsync();
                    }
                    _logger.LogInformation("Update IPs background process completed");

                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error on execute Update IPs background process");
                    throw new Exception("An error occurred while executing Update IPs background process", ex);
                }
            }
        }
    }
}
