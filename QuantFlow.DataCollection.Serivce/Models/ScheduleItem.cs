namespace QuantFlow.WorkerService.DataCollection.Models;

public class ScheduleItem
{
    public int IntervalMinutes { get; set; }
    public int LookbackMinutes { get; set; }
    public bool Enabled { get; set; } = true;
    public string CronExpression { get; set; } = string.Empty;
}