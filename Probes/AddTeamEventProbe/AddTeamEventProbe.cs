using OnCallProber.Services;

namespace OnCallProber.Probes.AddTeamEventProbe;

internal class AddTeamEventProbe : ProbeJob
{
    private readonly ILogger<AddTeamEventProbe> _logger;
    private readonly IDefaultProbeMetricExporter _metricExporter;
    private readonly EventsService _eventsService;

    public AddTeamEventProbe(
        [FromKeyedServices("AddTeamEventProbeExporter")] IDefaultProbeMetricExporter defaultExporter,
        ILogger<AddTeamEventProbe> logger,
        EventsService service
        )
    {
        _metricExporter = defaultExporter;
        _logger = logger;
        _eventsService = service;
    }
    
    protected override async Task ProbeAsync()
    {
        var startTime = DateTimeOffset.UtcNow.AddDays(1);
        var endTime = startTime.AddMinutes(5);
        var newEvent = new EventsService.CreateEvent
        {
            Role = "primary",
            Team = "test-team",
            User = "root",
            StartTimestamp = startTime.ToUnixTimeSeconds(),
            EndTimestamp = endTime.ToUnixTimeSeconds(),
        };
        _logger.LogDebug("Attempting to create new event {Event} for test team.", newEvent);
        _metricExporter.IncProbeAttempt();

        var timer = _metricExporter.NewTimer();
        var eventCreated = await _eventsService.CreateNewEventAsync(newEvent);
        timer.Dispose();

        if (eventCreated)
        {
            _logger.LogDebug("Created new event {Event}.", newEvent);
            _metricExporter.IncProbeSuccess();
        }
        else
        {
            _logger.LogDebug("Failed to create new event {Event}.", newEvent);
        }
    }
}