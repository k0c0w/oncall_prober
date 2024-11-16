using Microsoft.Extensions.Options;

namespace OnCallProber.Services;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        return services.AddScoped<UserService>()
            .AddScoped<EventsService>()
            .AddSingleton<SignatureEncoder>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<OnCallInfo>>().Value;
                return new SignatureEncoder(settings.AppKey);
            })
            .AddHttpClient()
            .AddLogging(cfg => cfg.AddConsole());
    }
}