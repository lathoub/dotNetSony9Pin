﻿using lathoub.dotNetSony9Pin.Sony9Pin.CommandBlocks;

namespace lathoub.dotNetSony9Pin.Sony9Pin.CommandBlocks.PresetSelectControl;

/// <summary>
/// 
/// </summary>
public class OutMinusShift : CommandBlock
{
    /// <summary>
    ///     Decrements the Video out point by one frame.
    /// </summary>
    public OutMinusShift()
    {
        Cmd1 = Cmd1.PresetSelectControl;
        Cmd2 = (byte)PresetSelectControl.OutMinusShift;
    }
}
