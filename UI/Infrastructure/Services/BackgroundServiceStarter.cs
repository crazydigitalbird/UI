namespace UI.Infrastructure.Services
{
    public class BackgroundServiceStarter<T> : IHostedService where T : IHostedService
    {
        private readonly T _backgroundService;

        public BackgroundServiceStarter(T backgroundService)
        {
            _backgroundService = backgroundService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return _backgroundService.StartAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _backgroundService.StopAsync(cancellationToken);
        }
    }
}
