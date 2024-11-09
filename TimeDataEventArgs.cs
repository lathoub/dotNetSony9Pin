using lathoub.dotNetSony9Pin.Sony9Pin.CommandBlocks.SenseReturn;

namespace lathoub.dotNetSony9Pin;

/// <summary>
///     The time data event args.
/// </summary>
public class TimeDataEventArgs : EventArgs
{
    #region Fields

    /// <summary>
    ///     The sense return.
    /// </summary>
    public readonly SenseReturn SenseReturn;

    /// <summary>
    ///     The time code.
    /// </summary>
    public readonly TimeCode? TimeCode;

    #endregion

    #region Constructors and Destructors

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeDataEventArgs"/> class.
    /// </summary>
    /// <param name="senseReturn">
    /// The sense return.
    /// </param>
    /// <param name="timeCode">
    /// The time code.
    /// </param>
    public TimeDataEventArgs(SenseReturn senseReturn, TimeCode? timeCode)
    {
        SenseReturn = senseReturn;
        TimeCode = timeCode;
    }

    #endregion
}