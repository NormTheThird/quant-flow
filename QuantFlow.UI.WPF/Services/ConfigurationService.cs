namespace QuantFlow.UI.WPF.Services;

public static class ConfigurationService
{
    public static IHost ConfigureServices()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(ConfigureAppConfiguration)
            .ConfigureServices(ConfigureServicesRegistration)
            .UseSerilog(ConfigureSerilog)
            .Build();

        return host;
    }

    private static void ConfigureAppConfiguration(HostBuilderContext context, IConfigurationBuilder config)
    {
        config.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true)
            .AddEnvironmentVariables();

        if (context.HostingEnvironment.IsDevelopment())
            config.AddUserSecrets<App>();
    }

    private static void ConfigureServicesRegistration(HostBuilderContext context, IServiceCollection services)
    {
        // Configuration
        services.AddSingleton(context.Configuration);

        // Data stores - only what we need
        services.AddSqlServerDataStore(context.Configuration);
        services.AddMongoDb(context.Configuration);

        // Domain services
        services.AddTransient<IUserService, UserService>();
        services.AddTransient<IKrakenApiService, KrakenApiService>();
        services.AddTransient<ISymbolService, SymbolService>();

        // HttpClient configuration
        services.AddHttpClient<IAuthenticationApiService, AuthenticationApiService>(client =>
        {
            client.BaseAddress = new Uri(context.Configuration["ApiBaseUrl"] ?? "https://localhost:7270");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        // Token storage
        services.AddSingleton<ITokenStorageService, TokenStorageService>();

        // Credential storage
        services.AddSingleton<ICredentialStorageService, CredentialStorageService>();

        // Theme service
        services.AddSingleton<IThemeService, ThemeService>();

        // User session service
        services.AddSingleton<IUserSessionService, UserSessionService>();

        // ViewModels
        services.AddTransient<LoginViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<SettingsViewModel>();

        // Views
        services.AddTransient<DashboardView>();
        services.AddTransient<SettingsView>();

        // Windows
        services.AddTransient<LoginWindow>();
        services.AddSingleton<MainWindow>();
    }

    private static void ConfigureSerilog(HostBuilderContext context, LoggerConfiguration loggerConfig)
    {
        loggerConfig.ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console();
    }
}