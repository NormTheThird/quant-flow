namespace QuantFlow.WorkerService.DataCollection.Configuration;

/// <summary>
/// Collection schedule configuration optimized for hourly collection cycles
/// Eventually this will be stored in SQL database for dynamic scheduling
/// </summary>
public class CollectionScheduleConfiguration
{
    public const string SectionName = "CollectionSchedule";

    /// <summary>
    /// 1-minute data collection - every hour with 60-minute lookback
    /// </summary>
    public ScheduleItem OneMinute { get; set; } = new()
    {
        Enabled = true,
        IntervalMinutes = 60,      // Collect every hour
        LookbackMinutes = 60       // Get last 60 minutes
    };

    /// <summary>
    /// 5-minute data collection - every hour with 100-minute lookback (20 periods)
    /// </summary>
    public ScheduleItem FiveMinute { get; set; } = new()
    {
        Enabled = true,
        IntervalMinutes = 60,      // Collect every hour
        LookbackMinutes = 100      // Get last 20 periods
    };

    /// <summary>
    /// 15-minute data collection - every hour with 60-minute lookback (4 periods)
    /// </summary>
    public ScheduleItem FifteenMinute { get; set; } = new()
    {
        Enabled = true,
        IntervalMinutes = 60,      // Collect every hour
        LookbackMinutes = 60       // Get last 4 periods
    };

    /// <summary>
    /// 1-hour data collection - every hour with 2-hour lookback
    /// </summary>
    public ScheduleItem OneHour { get; set; } = new()
    {
        Enabled = true,
        IntervalMinutes = 60,      // Collect every hour
        LookbackMinutes = 120      // Get last 2 hours
    };

    /// <summary>
    /// 4-hour data collection - every 4 hours with 8-hour lookback (2 periods)
    /// Collected at: 00:00, 04:00, 08:00, 12:00, 16:00, 20:00 UTC
    /// </summary>
    public ScheduleItem FourHour { get; set; } = new()
    {
        Enabled = true,
        IntervalMinutes = 240,     // Every 4 hours
        LookbackMinutes = 480      // Get last 8 hours (2 periods)
    };

    /// <summary>
    /// Daily data collection - once per day at 00:00 UTC with 2-day lookback
    /// </summary>
    public ScheduleItem OneDay { get; set; } = new()
    {
        Enabled = true,
        IntervalMinutes = 1440,    // Once per day
        LookbackMinutes = 2880     // Get last 2 days
    };
}