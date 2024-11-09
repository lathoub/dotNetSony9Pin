﻿namespace lathoub.dotNetSony9Pin.Sony9Pin.CommandBlocks.TransportControl;

/// <summary>
/// 
/// </summary>
public class JogRev : CommandBlock
{
    /// <summary>
    ///     When receiving one of the above commands, the _slave will start running in accordance with the speed Data defined
    ///     by DATA-1 and DATA-2. For the maximum and minimum speed see the 2X.12 Shuttle Fwd command.
    /// </summary>
    public JogRev()
    {
        Cmd1 = Cmd1.TransportControl;
        Cmd2 = (byte)TransportControl.JogRev;
    }
}
