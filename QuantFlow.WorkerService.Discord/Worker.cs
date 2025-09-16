namespace QuantFlow.WorkerService.Discord;

/// <summary>
/// Background service that manages the Discord bot connection and handles Discord events
/// </summary>
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly DiscordClient _discordClient;

    /// <summary>
    /// Initializes a new instance of the Worker class
    /// </summary>
    /// <param name="logger">Logger instance for structured logging</param>
    /// <param name="discordClient">Discord client for bot operations</param>
    /// <exception cref="ArgumentNullException">Thrown when logger or discordClient is null</exception>
    public Worker(ILogger<Worker> logger, DiscordClient discordClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _discordClient = discordClient ?? throw new ArgumentNullException(nameof(discordClient));
    }

    /// <summary>
    /// Starts the Discord bot service asynchronously
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete</param>
    /// <returns>A task that represents the asynchronous start operation</returns>
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Discord Bot Worker");

        try
        {
            // Register event handlers
            _discordClient.MessageCreated += OnMessageCreated;
            _discordClient.Ready += OnReady;

            // Connect to Discord
            await _discordClient.ConnectAsync();
            _logger.LogInformation("Discord client connected successfully");

            await base.StartAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start Discord Bot Worker");
            throw;
        }
    }

    /// <summary>
    /// Stops the Discord bot service asynchronously
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete</param>
    /// <returns>A task that represents the asynchronous stop operation</returns>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Discord Bot Worker");

        try
        {
            // Unregister event handlers to prevent memory leaks
            _discordClient.MessageCreated -= OnMessageCreated;
            _discordClient.Ready -= OnReady;

            // Disconnect from Discord
            await _discordClient.DisconnectAsync();
            _logger.LogInformation("Discord client disconnected successfully");

            // Dispose of the client
            _discordClient.Dispose();

            await base.StopAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while stopping Discord Bot Worker");
            throw;
        }
    }

    /// <summary>
    /// Executes the background service. Keeps the service running until cancellation is requested
    /// </summary>
    /// <param name="stoppingToken">Cancellation token to observe while waiting for the task to complete</param>
    /// <returns>A task that represents the long-running operations</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogDebug("Discord Bot Worker is running");

        // Keep the service alive while not cancelled
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }

        _logger.LogDebug("Discord Bot Worker execution completed");
    }

    /// <summary>
    /// Handles the Discord client ready event
    /// </summary>
    /// <param name="sender">The Discord client that triggered the event</param>
    /// <param name="e">Event arguments containing ready information</param>
    /// <returns>A task that represents the asynchronous event handling operation</returns>
    private async Task OnReady(DiscordClient sender, ReadyEventArgs e)
    {
        _logger.LogInformation("Discord bot is ready. Logged in as {Username}#{Discriminator}",
            sender.CurrentUser.Username, sender.CurrentUser.Discriminator);

        // Register slash commands here if needed in the future
        _logger.LogDebug("Discord bot ready event completed");

        await Task.CompletedTask;
    }

    /// <summary>
    /// Handles incoming Discord messages and processes bot commands
    /// </summary>
    /// <param name="sender">The Discord client that received the message</param>
    /// <param name="e">Event arguments containing message information</param>
    /// <returns>A task that represents the asynchronous message handling operation</returns>
    private async Task OnMessageCreated(DiscordClient sender, MessageCreateEventArgs e)
    {
        // Ignore messages from bots to prevent infinite loops
        if (e.Author.IsBot)
            return;

        try
        {
            _logger.LogDebug("Processing message from {Username}: {Content}",
                e.Author.Username, e.Message.Content);

            // Handle different message commands
            switch (e.Message.Content.ToLowerInvariant())
            {
                case "ping":
                    await HandlePingCommand(e);
                    break;

                case "jason is":
                    await HandleJasonCommand(e);
                    break;

                default:
                    // Log unhandled messages for debugging purposes
                    _logger.LogTrace("Unhandled message: {Content}", e.Message.Content);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message from {Username}", e.Author.Username);

            try
            {
                await e.Message.RespondAsync("Sorry, an error occurred while processing your message.");
            }
            catch (Exception responseEx)
            {
                _logger.LogError(responseEx, "Failed to send error response to user {Username}", e.Author.Username);
            }
        }
    }

    /// <summary>
    /// Handles the "ping" command by responding with "Pong"
    /// </summary>
    /// <param name="e">Message event arguments</param>
    /// <returns>A task that represents the asynchronous command handling operation</returns>
    private async Task HandlePingCommand(MessageCreateEventArgs e)
    {
        _logger.LogInformation("Ping command received from {Username}", e.Author.Username);
        await e.Message.RespondAsync("Pong");
    }

    /// <summary>
    /// Handles the "Jason Is" command with a humorous response
    /// </summary>
    /// <param name="e">Message event arguments</param>
    /// <returns>A task that represents the asynchronous command handling operation</returns>
    private async Task HandleJasonCommand(MessageCreateEventArgs e)
    {
        _logger.LogInformation("Jason command received from {Username}", e.Author.Username);
        await e.Message.RespondAsync("LAME!!!!!!");
    }
}