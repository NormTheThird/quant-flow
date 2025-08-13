namespace QuantFlow.WorkerService.DataCollection.Interfaces;

public interface IDataCollectionOrchestrator
{
    Task CollectRecentDataAsync(IEnumerable<string> symbols, IEnumerable<string> exchanges, IEnumerable<string> timeframes,
        DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default);
}