//// Program.cs - MongoDB integration example
//using QuantFlow.Data.MongoDB.Extensions;
//using QuantFlow.Data.Sql.Extensions;

//var builder = WebApplication.CreateBuilder(args);

//// Validate MongoDB configuration early
//ServiceCollectionExtensions.ValidateMongoDbConfiguration(builder.Configuration);

//// Add services to the container
//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//// Configure MongoDB serialization first
//builder.Services.ConfigureMongoDbSerialization();

//// Add SQL Server data store
//builder.Services.AddSqlServer(builder.Configuration);

//// Add MongoDB data store
//builder.Services.AddMongoDb(builder.Configuration);

//// Add health checks for both databases
//builder.Services.AddSqlServerHealthChecks(builder.Configuration);
//builder.Services.AddMongoDbHealthChecks(builder.Configuration);

//// Add logging
//builder.Services.AddLogging(loggingBuilder =>
//{
//    loggingBuilder.AddConsole();
//    loggingBuilder.AddDebug();
//});

//var app = builder.Build();

//// Configure the HTTP request pipeline
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();
//app.UseAuthorization();

//// Add health check endpoints
//app.MapHealthChecks("/health");
//app.MapHealthChecks("/health/mongodb", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
//{
//    Predicate = check => check.Tags.Contains("mongodb")
//});
//app.MapHealthChecks("/health/sqlserver", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
//{
//    Predicate = check => check.Tags.Contains("sqlserver")
//});

//app.MapControllers();

//// Ensure databases are ready
//await EnsureDatabasesReady(app);

//app.Run();

///// <summary>
///// Ensures both SQL Server and MongoDB are ready
///// </summary>
///// <param name="app">Web application</param>
//static async Task EnsureDatabasesReady(WebApplication app)
//{
//    using var scope = app.Services.CreateScope();
//    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

//    try
//    {
//        // Check SQL Server connection
//        var sqlContext = scope.ServiceProvider.GetRequiredService<QuantFlow.Data.Sql.ApplicationDbContext>();
//        await sqlContext.Database.CanConnectAsync();
//        logger.LogInformation("SQL Server connection verified");

//        // Check MongoDB connection
//        var mongoContext = scope.ServiceProvider.GetRequiredService<QuantFlow.Data.MongoDB.Context.MongoDbContext>();
//        var isHealthy = await mongoContext.IsHealthyAsync();

//        if (isHealthy)
//        {
//            logger.LogInformation("MongoDB connection verified");
//        }
//        else
//        {
//            logger.LogWarning("MongoDB connection check failed");
//        }
//    }
//    catch (Exception ex)
//    {
//        logger.LogError(ex, "Database connection verification failed");
//        // Don't stop the application, let health checks handle it
//    }
//}