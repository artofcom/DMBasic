using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//TODO Localization
namespace NOVNINE
{

public static class DateTimeHelper
{
    private const int SECOND = 1;
    private const int MINUTE = 60 * SECOND;
    private const int HOUR = 60 * MINUTE;
    private const int DAY = 24 * HOUR;
    private const int MONTH = 30 * DAY;

    /// <summary>
    /// Returns a friendly version of the provided DateTime, relative to now. E.g.: "2 days ago", or "in 6 months".
    /// </summary>
    /// <param name="dateTime">The DateTime to compare to Now</param>
    /// <returns>A friendly string</returns>
    public static string GetFriendlyRelativeTime(DateTime dateTime)
    {
        if (DateTime.UtcNow.Ticks == dateTime.Ticks) {
            return "Right Now!";
        }

        bool isFuture = (DateTime.UtcNow.Ticks < dateTime.Ticks);
        var ts = DateTime.UtcNow.Ticks < dateTime.Ticks ? new TimeSpan(dateTime.Ticks - DateTime.UtcNow.Ticks) : new TimeSpan(DateTime.UtcNow.Ticks - dateTime.Ticks);

        double delta = ts.TotalSeconds;

        if (delta < 1 * MINUTE) {
            return isFuture ? "in " + (ts.Seconds == 1 ? "one second" : ts.Seconds + " seconds") : ts.Seconds == 1 ? "One Second Ago" : ts.Seconds + " Seconds Ago";
        }
        if (delta < 2 * MINUTE) {
            return isFuture ? "In A Minute" : "A Minute Ago";
        }
        if (delta < 45 * MINUTE) {
            return isFuture ? "In " + ts.Minutes + " Minutes" : ts.Minutes + " Minutes Ago";
        }
        if (delta < 90 * MINUTE) {
            return isFuture ? "In An Hour" : "An Hour Ago";
        }
        if (delta < 24 * HOUR) {
            return isFuture ? "In " + ts.Hours + " Hours" : ts.Hours + " Hours Ago";
        }
        if (delta < 48 * HOUR) {
            return isFuture ? "Tomorrow" : "Yesterday";
        }
        if (delta < 30 * DAY) {
            return isFuture ? "In " + ts.Days + " Days" : ts.Days + " Days Ago";
        }
        if (delta < 12 * MONTH) {
            int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
            return isFuture ? "In " + (months <= 1 ? "One Month" : months + " Months") : months <= 1 ? "One Month Ago" : months + " Months Ago";
        } else {
            int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
            return isFuture ? "In " + (years <= 1 ? "One Year" : years + " Years") : years <= 1 ? "One Year Ago" : years + " Years Ago";
        }
    }

    public static string ToRelativeDate(System.DateTime input)
    {
        System.TimeSpan oSpan = System.DateTime.UtcNow.Subtract(input);
        return ToRelativeDate(oSpan);
    }

    public static string ToRelativeDate(System.TimeSpan oSpan)
    {
        double TotalMinutes = oSpan.TotalMinutes;
        string Suffix = "";

        if (TotalMinutes < 0.0) {
            TotalMinutes = Mathf.Abs((float)TotalMinutes);
            Suffix = " from Now";
        }

        var aValue = new SortedList<double, System.Func<string>>();
        aValue.Add(0.75, () => "Seconds");
        aValue.Add(1.5, () => "1 Min");
        aValue.Add(45, () => string.Format("{0} Min", System.Math.Round(TotalMinutes)));
        aValue.Add(90, () => "1 Hr");
        aValue.Add(1440, () => string.Format("{0} Hrs", System.Math.Round(System.Math.Abs(oSpan.TotalHours)))); // 60 * 24
        aValue.Add(2880, () => "1 Day"); // 60 * 48
        aValue.Add(43200, () => string.Format("{0} Days", System.Math.Floor(System.Math.Abs(oSpan.TotalDays)))); // 60 * 24 * 30
        aValue.Add(86400, () => "a Month"); // 60 * 24 * 60
        aValue.Add(525600, () => string.Format("{0} Months", System.Math.Floor(System.Math.Abs(oSpan.TotalDays / 30)))); // 60 * 24 * 365
        aValue.Add(1051200, () => "a Year"); // 60 * 24 * 365 * 2
        aValue.Add(double.MaxValue, () => string.Format("{0} Years", System.Math.Floor(System.Math.Abs(oSpan.TotalDays / 365))));

        return aValue.First(n => TotalMinutes < n.Key).Value.Invoke() + Suffix;
    }

    public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
    {
        int diff = dt.DayOfWeek - startOfWeek;
        if (diff < 0) {
            diff += 7;
        }

        return dt.AddDays(-1 * diff).Date;
    }
}

}

