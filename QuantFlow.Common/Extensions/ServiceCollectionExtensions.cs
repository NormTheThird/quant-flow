namespace QuantFlow.Common.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the rate limit handler with configuration
    /// </summary>
    public static IServiceCollection AddRateLimitHandling(this IServiceCollection services, IConfiguration configuration)
    {
        // Alternative approach - manually bind the configuration
        var rateLimitSettings = new RateLimitSettings();
        configuration.GetSection(RateLimitSettings.SectionName).Bind(rateLimitSettings);

        // Register as singleton instance
        services.AddSingleton(Options.Create(rateLimitSettings));

        // Register the rate limit handler as singleton
        services.AddSingleton<IApiRateLimitHandler, ApiRateLimitHandler>();

        return services;
    }
}