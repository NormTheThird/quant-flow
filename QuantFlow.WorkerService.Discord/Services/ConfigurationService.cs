namespace QuantFlow.WorkerService.Discord.Services;

/// <summary>
/// Configuration service for setting up dependency injection and configuration for the Discord bot
/// </summary>
public static class ConfigurationService
{
    /// <summary>
    /// Configures the host builder with all necessary services and configuration
    /// </summary>
    /// <param name="hostBuilder">The host builder to configure</param>
    /// <param name="args">Command line arguments</param>
    /// <returns>Configured host builder</returns>
    public static IHostBuilder ConfigureApplication(this IHostBuilder hostBuilder, string[] args)
    {
        return hostBuilder.ConfigureAppConfiguration(args)
            .ConfigureServices();
    }

    /// <summary>
    /// Configures application configuration sources
    /// </summary>
    /// <param name="hostBuilder">The host builder to configure</param>
    /// <param name="args">Command line arguments</param>
    /// <returns>Host builder with configured application configuration</returns>
    public static IHostBuilder ConfigureAppConfiguration(this IHostBuilder hostBuilder, string[] args)
    {
        return hostBuilder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args);

            // Add User Secrets in Development
            if (context.HostingEnvironment.IsDevelopment())
            {
                config.AddUserSecrets<Program>();

                // Add Vault for local development
                config.AddVault("http://vault.local:30420", "quant-flow", "root");
            }
            else
            {
                // Add Vault for production
                config.AddVault("http://vault.vault.svc.cluster.local:8200", "quant-flow", "root");
            }
        });
    }

    /// <summary>
    /// Configures dependency injection services for the Discord bot
    /// </summary>
    /// <param name="hostBuilder">The host builder to configure</param>
    /// <returns>Host builder with configured services</returns>
    public static IHostBuilder ConfigureServices(this IHostBuilder hostBuilder)
    {
        return hostBuilder.ConfigureServices((context, services) =>
        {
            services.AddSerilog(context, "Discord");

            // Register configuration as singleton
            services.AddSingleton(context.Configuration);

            // Discord client setup
            services.AddSingleton<DiscordClient>(serviceProvider =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var logger = serviceProvider.GetRequiredService<ILogger<DiscordClient>>();

                var token = configuration.GetValue<string>("Discord:Token");
                if (string.IsNullOrEmpty(token))
                {
                    throw new InvalidOperationException("Discord token is not configured. Please set Discord:Token in configuration.");
                }

                logger.LogInformation("Initializing Discord client");

                var discordConfig = new DiscordConfiguration
                {
                    Token = token,
                    TokenType = TokenType.Bot,
                    Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents,
                    UseRelativeRatelimit = true
                };

                var client = new DiscordClient(discordConfig);

                // Configure SlashCommands extension
                var slashCommands = client.UseSlashCommands(new SlashCommandsConfiguration
                {
                    Services = serviceProvider
                });

                // Register command modules
                slashCommands.RegisterCommands<ServerCommands>();

                // Optionally register for specific guild for faster testing
                var serverId = configuration.GetValue<ulong?>("Discord:ServerId");
                if (serverId.HasValue)
                {
                    slashCommands.RegisterCommands<ServerCommands>(serverId.Value);
                    logger.LogInformation("Registered slash commands for server (guild) {ServerId}", serverId.Value);
                }

                return client;
            });

            // Register the Discord bot worker service
            services.AddHostedService<Worker>();

            // Add HTTP client for any external API calls
            services.AddHttpClient();
        });
    }

}