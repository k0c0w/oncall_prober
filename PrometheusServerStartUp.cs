using Prometheus;

namespace OnCallProber;

public class PrometheusServerStartUp : BackgroundService
{
    private readonly ILogger<PrometheusServerStartUp> _logger;
    private readonly MetricServer _server;

    public PrometheusServerStartUp(MetricServer server, ILogger<PrometheusServerStartUp> logger)
    {
        _server = server;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        stoppingToken.Register(StopAndDisposeServer);
        _logger.LogInformation("Registered Prometheus server stop action");

        _logger.LogInformation("Starting Prometheus server");
        _server.Start();
        
        _logger.LogInformation("Prometheus server started");
    }

    private void StopAndDisposeServer()
    {
        _logger.LogInformation("Stopping Prometheus server");
        _server.Stop();
        _server.Dispose();
        
        _logger.LogInformation("Stopped Prometheus server");
    }
}