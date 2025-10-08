namespace QuantFlow.UI.WPF.Services;

public static class ConfigurationService
{
    public static IHost ConfigureServices()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                // Configuration
                services.AddSingleton(context.Configuration);

                // Data stores
                services.AddSqlServerDataStore(context.Configuration);

                // Domain services
                services.AddDomainServices();

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

                // ViewModels
                services.AddTransient<LoginViewModel>();

                // Windows
                services.AddTransient<LoginWindow>();
                services.AddSingleton<MainWindow>();
            })
            .UseSerilog((context, loggerConfig) =>
            {
                loggerConfig.ReadFrom.Configuration(context.Configuration)
                    .Enrich.FromLogContext()
                    .WriteTo.Console();
            })
            .Build();

        return host;
    }
}