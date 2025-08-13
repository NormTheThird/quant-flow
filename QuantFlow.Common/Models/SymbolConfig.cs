namespace QuantFlow.Common.Models;

public class SymbolConfig
{
    public string Symbol { get; set; } = string.Empty;
    public List<Timeframe> Timeframes { get; set; } = [];
}