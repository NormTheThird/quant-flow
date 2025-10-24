using QuantFlow.Common.Interfaces.Repositories.Mongo;

namespace QuantFlow.Data.MongoDB.Extensions;

/// <summary>
/// Extension methods for registering MongoDB services in the dependency injection container
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds MongoDB services to the service collection with proper configuration
    /// </summary>
    /// <param name="services">The service collection to configure</param>
    /// <param name="configuration">The application configuration</param>
    /// <returns>The service collection for method chaining</returns>
    /// <exception cref="InvalidOperationException">Thrown when required configuration is missing</exception>
    public static IServiceCollection AddMongoDb(this IServiceCollection services, IConfiguration configuration)
    {
        // Validate configuration early
        ValidateMongoDbConfiguration(configuration);

        // Get configuration values
        var connectionString = configuration.GetConnectionString("MongoDB")!;
        var databaseName = configuration["MongoDB:DatabaseName"]!;

        // Configure MongoDB serialization conventions first
        ConfigureMongoDbSerialization();

        // Register MongoDB client as singleton for connection pooling
        services.AddSingleton<IMongoClient>(serviceProvider =>
        {
            var logger = serviceProvider.GetRequiredService<ILogger<IMongoClient>>();
            logger.LogInformation("Initializing MongoDB client for database: {DatabaseName}", databaseName);

            try
            {
                var clientSettings = MongoClientSettings.FromConnectionString(connectionString);

                // Configure client settings for optimal performance
                clientSettings.ApplicationName = "QuantFlow";
                clientSettings.MaxConnectionPoolSize = 100;
                clientSettings.MinConnectionPoolSize = 5;
                clientSettings.ConnectTimeout = TimeSpan.FromSeconds(30);
                clientSettings.SocketTimeout = TimeSpan.FromSeconds(30);
                clientSettings.ServerSelectionTimeout = TimeSpan.FromSeconds(30);

                // Use server API version 1 for compatibility
                clientSettings.ServerApi = new ServerApi(ServerApiVersion.V1);

                // Configure retry settings
                clientSettings.RetryWrites = true;
                clientSettings.RetryReads = true;

                logger.LogDebug("MongoDB client configured successfully");
                return new MongoClient(clientSettings);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to initialize MongoDB client");
                throw;
            }
        });

        // Register MongoDB context as scoped for per-request lifecycle
        services.AddScoped<MongoDbContext>(serviceProvider =>
        {
            var client = serviceProvider.GetRequiredService<IMongoClient>();
            var logger = serviceProvider.GetRequiredService<ILogger<MongoDbContext>>();

            try
            {
                return new MongoDbContext(client, databaseName, logger);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to create MongoDB context for database: {DatabaseName}", databaseName);
                throw;
            }
        });

        // Register repository implementations
        services.AddTransient<ICustomAlgorithmRepository, CustomAlgorithmRepository>();
        services.AddTransient<IUserPreferencesRepository, UserPreferencesRepository>();
        services.AddTransient<IConfigurationRepository, ConfigurationRepository>();
        services.AddTransient<ITemplateRepository, TemplateRepository>();

        return services;
    }

    /// <summary>
    /// Adds MongoDB health checks to monitor database connectivity
    /// </summary>
    /// <param name="services">The service collection to configure</param>
    /// <param name="configuration">The application configuration</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddMongoDbHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MongoDB");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("MongoDB connection string is required for health checks");
        }

        // Use custom health check implementation to avoid assembly reference issues
        services.AddHealthChecks()
            .AddCheck<MongoDbHealthCheck>(
                "mongodb",
                failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
                tags: new[] { "mongodb", "database", "nosql" });

        return services;
    }

    /// <summary>
    /// Configures MongoDB serialization settings and conventions for optimal data handling
    /// </summary>
    /// <param name="services">The service collection to configure</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection ConfigureMongoDbSerialization(this IServiceCollection services)
    {
        ConfigureMongoDbSerialization();
        return services;
    }

    /// <summary>
    /// Configures MongoDB serialization settings and conventions
    /// </summary>
    private static void ConfigureMongoDbSerialization()
    {
        // Register custom serializers (with duplicate protection)
        if (!BsonSerializer.SerializerRegistry.GetSerializer<Guid>().GetType().Name.Contains("GuidSerializer"))
        {
            BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
        }

        if (!BsonSerializer.SerializerRegistry.GetSerializer<DateTime>().GetType().Name.Contains("DateTimeSerializer"))
        {
            BsonSerializer.RegisterSerializer(new DateTimeSerializer(BsonType.DateTime));
        }

        if (!BsonSerializer.SerializerRegistry.GetSerializer<decimal>().GetType().Name.Contains("DecimalSerializer"))
        {
            BsonSerializer.RegisterSerializer(new DecimalSerializer(BsonType.Decimal128));
        }

        // Configure global conventions (only if not already registered)
        if (!ConventionRegistry.Lookup(typeof(object)).Conventions.Any(c => c.GetType().Name.Contains("CamelCase")))
        {
            var conventionPack = new ConventionPack
            {
                // Use camelCase for element names (e.g., "userId" instead of "UserId")
                new CamelCaseElementNameConvention(),
                
                // Ignore extra elements in documents that don't have corresponding properties
                new IgnoreExtraElementsConvention(true),
                
                // Ignore null values when serializing to reduce document size
                new IgnoreIfNullConvention(true),
                
                // Use string representation for enums for better readability
                new EnumRepresentationConvention(BsonType.String)
            };

            // Apply conventions to all types in the QuantFlow namespace 
            ConventionRegistry.Register(
                "QuantFlowConventions",
                conventionPack,
                type => type.FullName?.StartsWith("QuantFlow") == true);
        }
    }

    /// <summary>
    /// Validates that all required MongoDB configuration values are present and valid
    /// </summary>
    /// <param name="configuration">The application configuration to validate</param>
    /// <exception cref="InvalidOperationException">Thrown when configuration is missing or invalid</exception>
    public static void ValidateMongoDbConfiguration(IConfiguration configuration)
    {
        var errors = new List<string>();

        // Validate connection string
        var connectionString = configuration.GetConnectionString("MongoDB");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            errors.Add("MongoDB connection string 'ConnectionStrings:MongoDB' is required");
        }
        else
        {
            // Validate connection string format
            try
            {
                var mongoUrl = new MongoUrl(connectionString);
            }
            catch (Exception ex)
            {
                errors.Add($"Invalid MongoDB connection string format: {ex.Message}");
            }
        }

        // Validate database name
        var databaseName = configuration["MongoDB:DatabaseName"];
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            errors.Add("MongoDB database name 'MongoDB:DatabaseName' is required");
        }

        // Throw exception if any validation errors occurred
        if (errors.Count > 0)
        {
            var errorMessage = string.Join(Environment.NewLine, errors);
            throw new InvalidOperationException($"MongoDB configuration validation failed:{Environment.NewLine}{errorMessage}");
        }
    }
}