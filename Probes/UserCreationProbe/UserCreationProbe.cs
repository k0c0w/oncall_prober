using OnCallProber.Services;
using Polly;

namespace OnCallProber.Probes.UserCreationProbe;

internal sealed class UserCreationProbe : ProbeJob
{
    private readonly IDefaultProbeMetricExporter _metricExporter;
    private readonly ILogger<UserCreationProbe> _logger;
    private readonly UserService _userService;

    public UserCreationProbe(
        [FromKeyedServices("UserCreationProbeExporter")] IDefaultProbeMetricExporter metricExporter,
        ILogger<UserCreationProbe> logger,
        UserService userService)
    {
        _logger = logger;
        _userService = userService;
        _metricExporter = metricExporter;
    }

    protected override async Task ProbeAsync()
    {
        var dummyUser = $"probe_user_{Ulid.NewUlid()}";
        _logger.LogDebug("Attempting to create new user {User}.", dummyUser);

        _metricExporter.IncProbeAttempt();

        var userHasBeenCreated = await TryCreateUserAsync(dummyUser);

        if (!userHasBeenCreated)
        {
            await TryCleanUpAsync(dummyUser);
        }
    }

    private async Task<bool> TryCreateUserAsync(string user)
    {
        var timer = _metricExporter.NewTimer();
        var userCreated = await _userService.CreateAsync(user);
        timer.Dispose();

        if (userCreated)
        {
            _logger.LogDebug("Created new user {User}.", user);
            _metricExporter.IncProbeSuccess();
        }
        else
        {
            _logger.LogDebug("Failed to create new user {User}.", user);
        }
        
        return userCreated;
    }

    private Task TryCleanUpAsync(string user)
    {
        _logger.LogDebug("Cleaning up after probe.");
        
        const int retriesCount = 3;
        var retryPolicy = Policy.HandleResult<bool>(deleted => !deleted)
            .WaitAndRetryAsync(retriesCount,
            retry => TimeSpan.FromSeconds(retry * retriesCount),
            onRetry: (exception, timeSpan, retry, context) =>
            {
                _logger.LogError("Failed to delete user {User} in {Attempt} attempt, retrying.", user, retry);
            });
        
        return retryPolicy.ExecuteAsync(async () => await _userService.DeleteAsync(user));
    }
}