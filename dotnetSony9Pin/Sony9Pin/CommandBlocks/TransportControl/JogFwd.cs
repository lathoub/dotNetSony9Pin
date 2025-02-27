﻿namespace dotNetSony9Pin.Sony9Pin.CommandBlocks.TransportControl;

/// <summary>
/// 
/// </summary>
public class JogFwd : CommandBlock
{
    /// <summary>
    ///  
    /// </summary>
    public JogFwd(byte n)
    {
        Cmd1 = CommandFunction.TransportControl;
        DataCount = 1;
        Cmd2 = (byte)TransportControl.JogFwd;
        Data = new[] { n };
    }

    /// <summary>
    /// 
    /// </summary>
    public JogFwd(byte n1, byte n2)
    {
        Cmd1 = CommandFunction.TransportControl;
        DataCount = 2;
        Cmd2 = (byte)TransportControl.JogFwd;
        Data = new[] { n1, n2 };
    }
}
