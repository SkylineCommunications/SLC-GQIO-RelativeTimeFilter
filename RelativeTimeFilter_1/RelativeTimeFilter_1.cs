using Skyline.DataMiner.Analytics.GenericInterface;
using System;
using System.Linq;

[GQIMetaData(Name = "Filter by relative time")]
public class RelativeTimeFilterOperator : IGQIRowOperator, IGQIInputArguments
{
    // Arguments
    private readonly GQIColumnDropdownArgument _startTimeArg;
    private readonly GQIColumnDropdownArgument _endTimeArg;
    private readonly GQIStringDropdownArgument _timeRangeArg;

    private readonly DateTime _now; // The moment in time to which the time range will be relative
    private readonly RelativeTimeRange[] _relativeTimeRanges;

    // Fields determined by the argument values
    private GQIColumn<DateTime> _startColumn;
    private GQIColumn<DateTime> _endColumn;
    private AbsoluteTimeRange _absoluteTimeRange;

    /// <summary>
    /// Initializes a new instance of the <see cref="RelativeTimeFilterOperator"/> class.
    /// Determines the moment in time to which the time range will be relative and initializes the input arguments.
    /// Called by GQI and should be parameterless.
    /// </summary>
    public RelativeTimeFilterOperator()
    {
        // Note: all DateTime objects provided by and returned to GQI need to be in UTC time
        _now = DateTime.UtcNow;

        _startTimeArg = new GQIColumnDropdownArgument("Start time")
        {
            IsRequired = true,
            Types = new[] { GQIColumnType.DateTime },
        };
        _endTimeArg = new GQIColumnDropdownArgument("End time")
        {
            IsRequired = false,
            Types = new[] { GQIColumnType.DateTime },
        };

        _relativeTimeRanges = GetRelativeTimeRanges();
        var timeRangeOptions = _relativeTimeRanges
            .Select(timeRange => timeRange.Name)
            .ToArray();
        _timeRangeArg = new GQIStringDropdownArgument("Time range", timeRangeOptions)
        {
            IsRequired = true,
            DefaultValue = timeRangeOptions[0],
        };
    }

    /// <summary>
    /// Called by GQI to define the input arguments.
    /// Defines and argument for the start time column, end time column and time range respectively.
    /// </summary>
    /// <returns>The defined arguments.</returns>
    public GQIArgument[] GetInputArguments()
    {
        return new GQIArgument[]
        {
            _startTimeArg,
            _endTimeArg,
            _timeRangeArg,
        };
    }

    /// <summary>
    /// Called by GQI to expose the chosen argument values.
    /// </summary>
    /// <param name="args">Collection of chosen argument values.</param>
    /// <returns>Unused.</returns>
    public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
    {
        _startColumn = (GQIColumn<DateTime>)args.GetArgumentValue(_startTimeArg);

        // Note: if no end column is specified, we use the start column as end time
        // This can be used to filter rows that have no duration
        if (args.TryGetArgumentValue(_endTimeArg, out var endColumn))
            _endColumn = (GQIColumn<DateTime>)endColumn;
        else
            _endColumn = _startColumn;

        // Determine the chosen time range
        var timeRangeName = args.GetArgumentValue(_timeRangeArg);
        var relativeTimeRange = GetRelativeTimeRangeByName(timeRangeName);
        _absoluteTimeRange = relativeTimeRange.GetAbsolute(_now);

        return default;
    }

    /// <summary>
    /// Called by GQI to handle each <paramref name="row"/> in turn.
    /// Retrieves the start and end time from the <paramref name="row"/> and checks if it overlaps the chosen <see cref="_absoluteTimeRange"/>.
    /// If there is no overlap, the row is marked to be deleted.
    /// </summary>
    /// <param name="row">The next row that needs to be handled.</param>
    public void HandleRow(GQIEditableRow row)
    {
        // Retrieve the start time
        var startTime = row.GetValue(_startColumn);

        // Retrieve the end time
        var endTime = row.GetValue(_endColumn);

        // Mark the row to be deleted if it does not overlap with the absolute time range
        var rowTimeRange = new AbsoluteTimeRange(startTime, endTime);
        if (!_absoluteTimeRange.Overlaps(rowTimeRange))
            row.Delete();
    }

    /// <summary>
    /// Defines all the supported relative time ranges.
    /// </summary>
    /// <returns>An array of relative time ranges.</returns>
    private static RelativeTimeRange[] GetRelativeTimeRanges()
    {
        return new RelativeTimeRange[]
        {
            RelativeTimeRange.AllTime,
            new RelativeTimeRange("Today", now =>
            {
                var end = now;
                var start = now.Date;
                return new AbsoluteTimeRange(start, end);
            }),
            new RelativeTimeRange("This month", now =>
            {
                var end = now;
                var start = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                return new AbsoluteTimeRange(start, end);
            }),
            new RelativeTimeRange("This year", now =>
            {
                var end = now;
                var start = new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                return new AbsoluteTimeRange(start, end);
            }),
            new RelativeTimeRange("Last hour", now =>
            {
                var end = now;
                var start = end.AddHours(-1);
                return new AbsoluteTimeRange(start, end);
            }),
            new RelativeTimeRange("Last 24 hours", now =>
            {
                var end = now;
                var start = end.AddHours(-24);
                return new AbsoluteTimeRange(start, end);
            }),
            new RelativeTimeRange("Yesterday", now =>
            {
                var end = now.Date;
                var start = end.AddDays(-1);
                return new AbsoluteTimeRange(start, end);
            }),
            new RelativeTimeRange("Last 7 days", now =>
            {
                var end = now.Date;
                var start = end.AddDays(-7);
                return new AbsoluteTimeRange(start, end);
            }),
            new RelativeTimeRange("Last 30 days", now =>
            {
                var end = now.Date;
                var start = end.AddDays(-30);
                return new AbsoluteTimeRange(start, end);
            }),
            new RelativeTimeRange("Tomorrow", now =>
            {
                var start = now.Date.AddDays(1);
                var end = start.AddDays(1);
                return new AbsoluteTimeRange(start, end);
            }),
            new RelativeTimeRange("Next 7 days", now =>
            {
                var start = now.Date.AddDays(1);
                var end = start.AddDays(7);
                return new AbsoluteTimeRange(start, end);
            }),
            new RelativeTimeRange("Next 30 days", now =>
            {
                var start = now.Date.AddDays(1);
                var end = start.AddDays(30);
                return new AbsoluteTimeRange(start, end);
            }),

            // Add more options here...
        };
    }

    /// <summary>
    /// Finds the relative time range by name.
    /// </summary>
    /// <param name="name">Name of the relative time range.</param>
    /// <returns>The matching relative time range, or <see cref="RelativeTimeRange.AllTime"/> otherwise.</returns>
    private RelativeTimeRange GetRelativeTimeRangeByName(string name)
    {
        return _relativeTimeRanges.FirstOrDefault(timeRange => timeRange.Name == name) ?? RelativeTimeRange.AllTime;
    }
}