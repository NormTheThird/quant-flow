try
{
    var host = Host.CreateDefaultBuilder(args)
        .ConfigureServices()
        .ConfigureLogging()
        .Build();

    var commandHandler = host.Services.GetRequiredService<ICommandHandler>();

    //var marketDataTestCases = new List<string[]>
    //{
    //    new[] { "market-data", "get", "--symbol", "BTCUSDT", "--timeframe", "1h", "--start", "2024-01-01", "--end", "2024-01-02", "--limit", "10" },
    //    new[] { "market-data", "latest", "--symbol", "ETHUSDT" },
    //    new[] { "market-data", "validate", "--symbol", "BTCUSDT", "--timeframe", "1d", "--start", "2024-01-01", "--end", "2024-01-07" },
    //    new[] { "market-data", "gaps", "--symbol", "ADAUSDT", "--timeframe", "1h", "--start", "2024-06-01", "--end", "2024-06-02" },
    //    new[] { "market-data", "availability", "--symbol", "DOTUSDT" }
    //};

    //foreach (var testArgs in marketDataTestCases)
    //{
    //    var result = await commandHandler.ExecuteAsync(testArgs);
    //    System.Console.WriteLine($"Test completed with exit code: {result}");
    //}

    //var exchangeConfigTestCases = new List<string[]>
    //{
    //    // Initialize default configurations
    //    new[] { "exchange-config", "init" },

    //    // List all exchanges
    //    new[] { "exchange-config", "list" },

    //    // Show Kraken configuration
    //    new[] { "exchange-config", "show", "--exchange", "kraken" },

    //    // Show KuCoin configuration  
    //    new[] { "exchange-config", "show", "--exchange", "kucoin" },

    //    // Set base fees for Kraken (real fees: Maker 0.25%, Taker 0.40%)
    //    new[] { "exchange-config", "set-base", "--exchange", "kraken", "--maker", "0.25", "--taker", "0.40" },

    //    // Set base fees for KuCoin (real fees: Maker 0.10%, Taker 0.10%)
    //    new[] { "exchange-config", "set-base", "--exchange", "kucoin", "--maker", "0.10", "--taker", "0.10" },

    //    // Add Kraken volume tier (50K volume: Maker 0.20%, Taker 0.35%)
    //    new[] { "exchange-config", "add-tier", "--exchange", "kraken", "--tier", "1", "--volume", "50000", "--maker", "0.20", "--taker", "0.35" },

    //    // Add KuCoin volume tier (50 BTC volume: Maker 0.08%, Taker 0.10%)
    //    new[] { "exchange-config", "add-tier", "--exchange", "kucoin", "--tier", "1", "--volume", "50", "--maker", "0.08", "--taker", "0.10" },

    //    // Add KuCoin high volume tier with maker rebate (-0.005% maker, 0.025% taker)
    //    new[] { "exchange-config", "add-tier", "--exchange", "kucoin", "--tier", "2", "--volume", "1000", "--maker", "-0.005", "--taker", "0.025" },

    //    // Set symbol override for Kraken stablecoin pair
    //    new[] { "exchange-config", "set-override", "--exchange", "kraken", "--symbol", "USDCUSD", "--maker", "0.20", "--taker", "0.20", "--reason", "Stablecoin pair discount" },

    //    // Show Kraken config with all tiers and overrides
    //    new[] { "exchange-config", "show", "--exchange", "kraken" },

    //    // Show KuCoin config with all tiers 
    //    new[] { "exchange-config", "show", "--exchange", "kucoin" },

    //    // Show specific symbol override
    //    new[] { "exchange-config", "show", "--exchange", "kraken", "--symbol", "USDCUSD" }
    //};

    //System.Console.WriteLine("Testing Exchange Configuration Service...");
    //System.Console.WriteLine(new string('=', 50));

    //foreach (var testArgs in exchangeConfigTestCases)
    //{
    //    System.Console.WriteLine($"\n> Command: {string.Join(" ", testArgs)}");
    //    System.Console.WriteLine(new string('-', 40));

    //    var result = await commandHandler.ExecuteAsync(testArgs);
    //    System.Console.WriteLine($"Exit code: {result}");

    //    if (result != 0)
    //        System.Console.WriteLine("❌ Command failed");
    //    else
    //        System.Console.WriteLine("✅ Command succeeded");
    //}
    
    await commandHandler.ExecuteCustomAsync();

    return 0; // Add this return statement
}
catch (Exception ex)
{
    System.Console.WriteLine($"Fatal error: {ex.Message}");
    return 1;
}