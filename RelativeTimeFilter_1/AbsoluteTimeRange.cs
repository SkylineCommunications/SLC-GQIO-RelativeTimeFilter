using System;

/// <summary>
/// Represents a time range with a fixed start time and end time.
/// </summary>
internal readonly struct AbsoluteTimeRange
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AbsoluteTimeRange"/> struct.
    /// </summary>
    /// <param name="start">The start time of the time range (inclusive).</param>
    /// <param name="end">The end time of the time range (exclusive).</param>
    public AbsoluteTimeRange(DateTime start, DateTime end)
    {
        Start = start;
        End = end;
    }

    /// <summary>
    /// Gets the start time of this time range (inclusive).
    /// </summary>
    public DateTime Start { get; }

    /// <summary>
    /// Gets the end time of this time range (exclusive).
    /// </summary>
    public DateTime End { get; }

    /// <summary>
    /// Checks whether the given <paramref name="timestamp"/> falls within this time range interval.
    /// </summary>
    /// <param name="timestamp">The timestamp to check.</param>
    /// <returns>True if the given timestamp falls within this time range, false otherwise.</returns>
    public bool Contains(DateTime timestamp)
    {
        return Start <= timestamp && timestamp < End;
    }

    /// <summary>
    /// Checks whether the given <paramref name="range"/> overlaps with this time range interval.
    /// </summary>
    /// <param name="range">The time range to check.</param>
    /// <returns>True if the given time range overlaps with this time range, false otherwise.</returns>
    public bool Overlaps(AbsoluteTimeRange range)
    {
        return Start < range.End && range.Start < End;
    }
}
