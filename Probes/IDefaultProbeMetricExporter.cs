namespace OnCallProber.Probes;

internal interface IDefaultProbeMetricExporter
{
    void IncProbeAttempt();

    void IncProbeSuccess();

    Prometheus.ITimer NewTimer();
}