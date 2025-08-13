namespace QuantFlow.Common.Extensions;

public static class DateTimeExtensions
{
    public static DateTime InCst(this DateTime dateTime)
    {
        if (dateTime.Kind == DateTimeKind.Local)
            return TimeZoneInfo.ConvertTime(dateTime, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));
        else if (dateTime.Kind == DateTimeKind.Utc)
            return TimeZoneInfo.ConvertTimeFromUtc(dateTime, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));
        else
            return dateTime;
    }

    public static DateTime EpochToUniversalDateTime(this long epochTime)
    {
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(epochTime).ToUniversalTime();
        return dateTime;
    }

    public static DateTime MillisecondEpochToUniversalDateTime(this long millisecondEpochTime)
    {
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddMilliseconds(millisecondEpochTime).ToUniversalTime();
        return dateTime;
    }

    public static DateTime ToCentralStandardTime(this long epochTime)
    {
        var utcDateTime = DateTimeOffset.FromUnixTimeMilliseconds(epochTime).UtcDateTime;
        var cstZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
        return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, cstZone);
    }

    public static long ToEpoch(this DateTime dateTime)
    {
        return (long)(dateTime.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0))).TotalSeconds;
    }

    public static long ToEpochSeconds(this DateTime dateTime)
    {
        return (long)(dateTime.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0))).TotalSeconds;
    }

    public static long ToEpochMilliseconds(this DateTime dateTime)
    {
        return (long)dateTime.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds;
    }
}