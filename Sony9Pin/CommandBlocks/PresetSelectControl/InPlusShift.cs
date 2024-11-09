﻿using lathoub.dotNetSony9Pin.Sony9Pin.CommandBlocks;

namespace lathoub.dotNetSony9Pin.Sony9Pin.CommandBlocks.PresetSelectControl;

/// <summary>
/// 
/// </summary>
public class InPlusShift : CommandBlock
{
    /// <summary>
    ///     Increments the Video in point by one frame.
    /// </summary>
    public InPlusShift()
    {
        Cmd1 = Cmd1.PresetSelectControl;
        Cmd2 = (byte)PresetSelectControl.InPlusShift;
    }
}
