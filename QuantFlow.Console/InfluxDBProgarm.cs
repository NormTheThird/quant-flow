//// Example usage in Program.cs or Startup.cs
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using QuantFlow.Data.InfluxDB.Extensions;
//using QuantFlow.Data.InfluxDB.HealthChecks;
//using QuantFlow.Common.Interfaces;

//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container
//builder.Services.AddLogging(logging =>
//{
//    logging.AddConsole();
//    logging.AddDebug();
//});

//// Add InfluxDB services
//builder.Services.AddCompleteInfluxDb(builder.Configuration);

//// Add health checks
//builder.Services.AddHealthChecks()
//    .AddCheck<InfluxDbHealthCheck>("influxdb");

//// Add controllers, etc.
//builder.Services.AddControllers();

//var app = builder.Build();

//// Configure the HTTP request pipeline
//if (app.Environment.IsDevelopment())
//{
//    app.UseDeveloperExceptionPage();
//}

//app.UseRouting();
//app.UseHealthChecks("/health");

//app.MapControllers();

//// Example usage
//app.MapGet("/example-usage", async (IMarketDataRepository marketDataRepo,
//                                  IAlgorithmPerformanceRepository perfRepo,
//                                  ISystemMetricsRepository systemRepo) =>
//{
//    try
//    {
//        // Example: Write some market data
//        var marketData = new QuantFlow.Common.Models.MarketDataModel
//        {
//            Symbol = "BTCUSD",
//            Exchange = "binance",
//            Timeframe = "1m",
//            Open = 45000m,
//            High = 45100m,
//            Low = 44900m,
//            Close = 45050m,
//            Volume = 100.5m,
//            Timestamp = DateTime.UtcNow
//        };
//        await marketDataRepo.WritePriceDataAsync(marketData);

//        // Example: Write performance metrics
//        var performance = new QuantFlow.Common.Models.AlgorithmPerformanceModel
//        {
//            AlgorithmId = Guid.NewGuid(),
//            AlgorithmName = "Example Strategy",
//            Version = "1.0.0",
//            Environment = "prod",
//            Symbol = "BTCUSD",
//            Timeframe = "1m",
//            PnL = 150.75m,
//            CumulativePnL = 1205.50m,
//            ReturnPercentage = 2.5m,
//            TradeCount = 5,
//            WinningTrades = 3,
//            LosingTrades = 2,
//            Timestamp = DateTime.UtcNow
//        };
//        await perfRepo.WritePerformanceMetricsAsync(performance);

//        // Example: Write system metrics
//        var systemMetrics = new QuantFlow.Common.Models.SystemMetricsModel
//        {
//            Host = Environment.MachineName,
//            Service = "quantflow-api",
//            Instance = "instance-1",
//            Environment = "prod",
//            CpuPercentage = 45.2m,
//            MemoryUsedMb = 512m,
//            MemoryTotalMb = 2048m,
//            MemoryPercentage = 25.0m,
//            Timestamp = DateTime.UtcNow
//        };
//        await systemRepo.WriteSystemMetricsAsync(systemMetrics);

//        // Example: Read latest market data
//        var latestPrice = await marketDataRepo.GetLatestPriceAsync("BTCUSD");

//        return Results.Ok(new
//        {
//            message = "InfluxDB operations completed successfully",
//            latestPrice = latestPrice?.Close,
//            timestamp = DateTime.UtcNow
//        });
//    }
//    catch (Exception ex)
//    {
//        return Results.Problem($"Error: {ex.Message}");
//    }
//});

//app.Run();

///* 
//Example appsettings.json configuration:

//{
//  "InfluxDb": {
//    "Url": "http://localhost:8086",
//    "Token": "your-influxdb-token-here",
//    "Bucket": "quantflow",
//    "Organization": "your-org",
//    "TimeoutSeconds": 30,
//    "EnableDebugLogging": true,
//    "BatchSize": 1000,
//    "FlushIntervalMs": 1000,
//    "MaxRetries": 3
//  },
//  "Logging": {
//    "LogLevel": {
//      "Default": "Information",
//      "QuantFlow.Data.InfluxDB": "Debug"
//    }
//  }
//}
//*/