﻿namespace dotNetSony9Pin.Sony9Pin.CommandBlocks;

/// <summary>
/// 
/// </summary>
public class UserBitPreset : CommandBlock
{
    /// <summary>
    ///     This command presets the value given by DATA-1 through DATA-4 to the User Bits of the Time Code Generator.
    /// </summary>
    public UserBitPreset()
    {
        Cmd1 = CommandFunction.TransportControl;
        //           this.Cmd2 = (byte)TransportControl.UserBitPreset;
        throw new NotImplementedException();
    }
}
