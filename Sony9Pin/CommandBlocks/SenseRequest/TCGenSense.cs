﻿using lathoub.dotNetSony9Pin.Sony9Pin.CommandBlocks;

namespace lathoub.dotNetSony9Pin.Sony9Pin.CommandBlocks.SenseRequest;

/// <summary>
/// 
/// </summary>
public class TcGenSense : CommandBlock
{
    /// <summary>
    /// </summary>
    public TcGenSense(TimeSenseRequest.TimeSenseRequest request)
    {
        var data = new[] { (byte)request };

        Cmd1 = Cmd1.SenseRequest;
        DataCount = data.Length;
        Cmd2 = (byte)SenseRequest.TcGenSense;
        Data = data;
    }
}
