namespace dotNetSony9Pin.Sony9Pin.CommandBlocks.TransportControl;

/// <summary>
/// 
/// </summary>
public class FreezeOn : CommandBlock
{
    /// <summary>
    ///     This command freezes the output of the device. There is usually a 2-5 frame delay associcated with the actual
    ///     freeze.
    /// </summary>
    public FreezeOn()
    {
        Cmd1 = CommandFunction.TransportControl;
        Cmd2 = (byte)TransportControl.FreezeOn;
    }
}