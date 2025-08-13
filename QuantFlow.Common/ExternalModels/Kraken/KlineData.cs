namespace QuantFlow.Common.ExternalModels.Kraken;

public class KlineData
{
    public long ChartTimeEpoch { get; set; } = 0;
    public decimal OpeningPrice { get; set; } = 0.0m;
    public decimal ClosingPrice { get; set; } = 0.0m;
    public decimal HighestPrice { get; set; } = 0.0m;
    public decimal LowestPrice { get; set; } = 0.0m;
    public decimal Volume { get; set; } = 0.0m;
    public decimal VolumeWeightedAveragePrice { get; set; } = 0.0m;
    public int NumberOfTrades { get; set; } = 0;
}