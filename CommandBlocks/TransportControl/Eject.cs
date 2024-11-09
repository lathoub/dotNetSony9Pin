﻿namespace Pynch.Sony9Pin.Core.Sony9Pin.CommandBlocks.TransportControl;

/// <summary>
/// 
/// </summary>
public class Eject : CommandBlock
{
    /// <summary>
    ///     When this command is received, the _slave will Eject the tape.
    /// </summary>
    public Eject()
    {
        Cmd1 = Cmd1.TransportControl;
        Cmd2 = (byte)TransportControl.Eject;
    }
}
