using OnCallProber;
using OnCallProber.Probes;
using OnCallProber.Probes.AddTeamEventProbe;
using OnCallProber.Probes.UserCreationProbe;
using OnCallProber.Services;
using Prometheus;
using Quartz;

var builder = Host.CreateApplicationBuilder(args);
var configuration = builder.Configuration;
var services = builder.Services;
var quartzSection = configuration.GetRequiredSection("Quartz");

services.Configure<QuartzOptions>(quartzSection)
    .AddQuartz(quartz =>
    {
        var probesConfiguration = quartzSection.GetRequiredSection("Probes");

        quartz.ScheduleJob<UserCreationProbe>(trigger => trigger
            .WithIdentity(nameof(UserCreationProbe))
            .StartNow()
            .WithSimpleSchedule(sch => sch
                .WithIntervalInSeconds(
                    probesConfiguration.GetSection($"{nameof(UserCreationProbe)}")
                        ?.GetValue<int>("IntervalInSeconds")
                    ?? 10)
                .RepeatForever()
                .Build())
        );
        
        quartz.ScheduleJob<AddTeamEventProbe>(trigger => trigger
            .WithIdentity(nameof(AddTeamEventProbe))
            .StartNow()
            .WithSimpleSchedule(sch => sch
                .WithIntervalInSeconds(
                    probesConfiguration.GetSection($"{nameof(AddTeamEventProbe)}")
                        ?.GetValue<int>("IntervalInSeconds")
                    ?? 10)
                .RepeatForever()
                .Build())
        );
    })
    .AddUserCreationProbeProbeJob()
    .AddAddTeamEventProbeJob()
    .AddHostedService<QuartzHostedService>();

services.AddSingleton<MetricServer>(sp =>
    {
        var prometheusServerConfiguration = configuration.GetRequiredSection("Prometheus");
        var port = prometheusServerConfiguration.GetValue<int>("Port");
        var url = prometheusServerConfiguration.GetValue<string?>("Url") ?? "metrics/";
        var useHttps = prometheusServerConfiguration.GetValue<bool?>("UseHttps") ?? false;

        Metrics.SuppressDefaultMetrics(new SuppressDefaultMetricOptions
        {
            SuppressEventCounters = true,
            SuppressMeters = true,
            SuppressProcessMetrics = true,
            SuppressDebugMetrics = true
        });

        return new MetricServer(port, url: url, useHttps: useHttps);
    })
    .AddHostedService<PrometheusServerStartUp>();

services.Configure<OnCallInfo>(configuration.GetRequiredSection("OnCall"))
    .AddServices();

var host = builder.Build();
await host.RunAsync();