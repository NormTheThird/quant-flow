using QuantFlow.Common.Interfaces.Repositories.SQLServer;

namespace QuantFlow.Domain.Services;

/// <summary>
/// Service for trade operations
/// </summary>
public class TradeService : ITradeService
{
    private readonly ITradeRepository _tradeRepository;
    private readonly ILogger<TradeService> _logger;

    public TradeService(
        ILogger<TradeService> logger,
        ITradeRepository tradeRepository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tradeRepository = tradeRepository ?? throw new ArgumentNullException(nameof(tradeRepository));
    }

    public async Task<TradeModel?> GetTradeByIdAsync(Guid id)
    {
        return await _tradeRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<TradeModel>> GetTradesByBacktestRunIdAsync(Guid backtestRunId)
    {
        return await _tradeRepository.GetByBacktestRunIdAsync(backtestRunId);
    }

    public async Task<TradeModel> CreateTradeAsync(TradeModel trade)
    {
        return await _tradeRepository.CreateAsync(trade);
    }

    public async Task<IEnumerable<TradeModel>> CreateTradesBatchAsync(IEnumerable<TradeModel> trades)
    {
        return await _tradeRepository.CreateBatchAsync(trades);
    }

    public async Task<TradeModel> UpdateTradeAsync(TradeModel trade)
    {
        return await _tradeRepository.UpdateAsync(trade);
    }

    public async Task<bool> DeleteTradeAsync(Guid id)
    {
        return await _tradeRepository.DeleteAsync(id);
    }
}