using OnCallProber.Probes.UserCreationProbe;

namespace OnCallProber.Probes;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUserCreationProbeProbeJob(
        this IServiceCollection services)
    {
        services.AddKeyedTransient<IDefaultProbeMetricExporter, UserCreationProbe.ProbeExporter>(
            "UserCreationProbeExporter");
        services.AddTransient<UserCreationProbe.UserCreationProbe>();
        
        return services;
    }
    
    public static IServiceCollection AddAddTeamEventProbeJob(
        this IServiceCollection services)
    {
        services.AddKeyedTransient<IDefaultProbeMetricExporter, AddTeamEventProbe.ProbeExporter>(
            "AddTeamEventProbeExporter");
        services.AddTransient<AddTeamEventProbe.AddTeamEventProbe>();
        
        return services;
    }
}