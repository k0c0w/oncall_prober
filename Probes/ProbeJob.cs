using Quartz;

namespace OnCallProber.Probes;

[DisallowConcurrentExecution]
internal abstract class ProbeJob : IJob
{
    protected abstract Task ProbeAsync();

    public Task Execute(IJobExecutionContext context) => ProbeAsync();
}