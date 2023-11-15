using System;

/// <summary>
/// Represents a time range independent from a specific point in time.
/// </summary>
internal sealed class RelativeTimeRange
{
    public RelativeTimeRange(string name, Func<DateTime, AbsoluteTimeRange> getAbsolute)
    {
        Name = name;
        GetAbsolute = getAbsolute;
    }

    /// <summary>
    /// Gets the maximum relative time range that contains all possible timestamps.
    /// </summary>
    public static RelativeTimeRange AllTime { get; } = new RelativeTimeRange("All time", _ =>
    {
        return new AbsoluteTimeRange(DateTime.MinValue, DateTime.MaxValue);
    });

    /// <summary>
    /// Gets the name of the relative time range that will be used to display as argument options.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the function that can convert this relative time range into an absolute time range, given a specific point in time.
    /// </summary>
    public Func<DateTime, AbsoluteTimeRange> GetAbsolute { get; }
}
