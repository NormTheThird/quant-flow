namespace QuantFlow.WorkerService.DataCollection.Infrastructure;

/// <summary>
/// Simple IOptionsMonitor implementation for hardcoded configuration classes
/// Provides compatibility with the IOptions pattern until database-driven configuration is implemented
/// </summary>
/// <typeparam name="T">Configuration class type</typeparam>
public class HardcodedOptionsMonitor<T> : IOptionsMonitor<T> where T : class
{
    private readonly T _configuration;

    public HardcodedOptionsMonitor(T configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public T CurrentValue => _configuration;

    public T Get(string? name) => _configuration;

    public IDisposable? OnChange(Action<T, string?> listener) => null; // No change notifications for hardcoded config
}