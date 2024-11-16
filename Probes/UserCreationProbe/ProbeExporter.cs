using Prometheus;

namespace OnCallProber.Probes.UserCreationProbe;

internal class ProbeExporter : IDefaultProbeMetricExporter
{
    private const string METRIC_NAME_PREFIX = "oncall_probe_user_create";
    
    private static Counter ScenarioTotalCounter { get; }
    
    private static Counter ScenarioSuccessCounter { get; }
    
    private static Gauge ScenarioDurationGauge { get; }
    
    static ProbeExporter()
    {
        string[] appLabels = ["oncall"];
        var counterConfiguration = new CounterConfiguration()
        {
            LabelNames = appLabels,
            SuppressInitialValue = false
        };
        
        ScenarioTotalCounter = Metrics.CreateCounter(
            name: $"{METRIC_NAME_PREFIX}_total",
            help: "Total number of probe executions", 
            counterConfiguration);
        ScenarioSuccessCounter = Metrics.CreateCounter(
            name: $"{METRIC_NAME_PREFIX}_success",
            help: "Number of successful probe executions",
            counterConfiguration);
        ScenarioDurationGauge = Metrics.CreateGauge(
            name: $"{METRIC_NAME_PREFIX}_duration",
            help: "Scenario execution time",
            labelNames: appLabels);
    }

    public void IncProbeAttempt() => ScenarioTotalCounter.Inc();
    
    public void IncProbeSuccess() => ScenarioSuccessCounter.Inc();

    public Prometheus.ITimer NewTimer() => ScenarioDurationGauge.NewTimer(); 
}