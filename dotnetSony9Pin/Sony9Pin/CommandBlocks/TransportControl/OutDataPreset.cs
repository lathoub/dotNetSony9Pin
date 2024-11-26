﻿namespace dotNetSony9Pin.Sony9Pin.CommandBlocks.TransportControl;

/// <summary>
/// 
/// </summary>
public class OutDataPreset : CommandBlock
{
    /// <summary>
    ///     Set the Video Out Point to the value indicated by DATA-1 thru DATA-4. The time format is as per the 24.31 Cue Up
    ///     With Data command.
    /// </summary>
    public OutDataPreset(TimeCode tc)
    {
        var data = tc.ToBinaryCodedDecimal();

        Cmd1DataCount = ToCmd1DataCount(CommandFunction.TransportControl, data.Length);
        Cmd2 = (byte)TransportControl.OutDataPreset;
        Data = data;
    }
}
