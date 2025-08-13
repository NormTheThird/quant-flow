namespace QuantFlow.WorkerService.DataCollection.Configuration;

public class CollectionScheduleConfiguration
{
    public const string SectionName = "CollectionSchedule";

    public ScheduleItem OneMinute { get; set; } = new() { IntervalMinutes = 1, LookbackMinutes = 5 };
    public ScheduleItem FiveMinute { get; set; } = new() { IntervalMinutes = 5, LookbackMinutes = 15 };
    public ScheduleItem FifteenMinute { get; set; } = new() { IntervalMinutes = 15, LookbackMinutes = 30 };
    public ScheduleItem OneHour { get; set; } = new() { IntervalMinutes = 60, LookbackMinutes = 120 };
    public ScheduleItem FourHour { get; set; } = new() { IntervalMinutes = 240, LookbackMinutes = 480 };
    public ScheduleItem OneDay { get; set; } = new() { IntervalMinutes = 1440, LookbackMinutes = 2880 };
}