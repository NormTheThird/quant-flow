//namespace QuantFlow.WorkerService.Discord.Services;

//public class DiscordCommandService : IDiscordCommandService
//{
//    private readonly ILogger<DiscordCommandService> _logger;
//    private readonly ITradingCommandHandler _tradingHandler;
//    private readonly IPortfolioCommandHandler _portfolioHandler;
//    private readonly IBacktestCommandHandler _backtestHandler;
//    private readonly DiscordConfiguration _config;

//    public DiscordCommandService(
//        ILogger<DiscordCommandService> logger,
//        ITradingCommandHandler tradingHandler,
//        IPortfolioCommandHandler portfolioHandler,
//        IBacktestCommandHandler backtestHandler,
//        DiscordConfiguration config)
//    {
//        _logger = logger;
//        _tradingHandler = tradingHandler;
//        _portfolioHandler = portfolioHandler;
//        _backtestHandler = backtestHandler;
//        _config = config;
//    }

//    public async Task HandleMessageAsync(MessageCreateEventArgs e)
//    {
//        var content = e.Message.Content.Trim();

//        if (!content.StartsWith(_config.CommandPrefix))
//            return;

//        var parts = content[_config.CommandPrefix.Length..].Split(' ', StringSplitOptions.RemoveEmptyEntries);
//        if (parts.Length == 0) return;

//        var command = parts[0].ToLowerInvariant();
//        var args = parts.Skip(1).ToArray();

//        _logger.LogInformation("Processing command {Command} from {Username}", command, e.Author.Username);

//        switch (command)
//        {
//            case "portfolio":
//            case "p":
//                await _portfolioHandler.HandlePortfolioCommand(e, args);
//                break;

//            case "price":
//                await _tradingHandler.HandlePriceCommand(e, args);
//                break;

//            case "backtest":
//            case "bt":
//                await _backtestHandler.HandleBacktestCommand(e, args);
//                break;

//            case "help":
//                await HandleHelpCommand(e);
//                break;

//            default:
//                await e.Message.RespondAsync($"Unknown command: {command}. Use `{_config.CommandPrefix}help` for available commands.");
//                break;
//        }
//    }

//    public async Task HandleSlashCommandAsync(SlashCommandExecutedEventArgs e)
//    {
//        var commandName = e.Interaction.Data.Name.ToLowerInvariant();

//        _logger.LogInformation("Processing slash command {Command} from {Username}",
//            commandName, e.Interaction.User.Username);

//        switch (commandName)
//        {
//            case "portfolio":
//                await _portfolioHandler.HandlePortfolioSlashCommand(e);
//                break;

//            case "price":
//                await _tradingHandler.HandlePriceSlashCommand(e);
//                break;

//            case "backtest":
//                await _backtestHandler.HandleBacktestSlashCommand(e);
//                break;

//            default:
//                await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
//                    new DiscordInteractionResponseBuilder()
//                        .WithContent($"Unknown command: {commandName}")
//                        .AsEphemeral());
//                break;
//        }
//    }

//    private async Task HandleHelpCommand(MessageCreateEventArgs e)
//    {
//        var embed = new DiscordEmbedBuilder()
//            .WithTitle("QuantFlow Trading Bot Commands")
//            .WithColor(DiscordColor.Blue)
//            .AddField($"{_config.CommandPrefix}portfolio", "View your trading portfolio")
//            .AddField($"{_config.CommandPrefix}price <symbol>", "Get current price for a symbol")
//            .AddField($"{_config.CommandPrefix}backtest <algorithm> <symbol>", "Run a backtest")
//            .WithFooter("QuantFlow Automated Trading Platform")
//            .WithTimestamp(DateTime.UtcNow);

//        await e.Message.RespondAsync(embed: embed);
//    }
//}