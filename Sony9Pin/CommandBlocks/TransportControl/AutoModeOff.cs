namespace lathoub.dotNetSony9Pin.Sony9Pin.CommandBlocks.TransportControl;

/// <summary>
/// 
/// </summary>
public class AutoModeOff : CommandBlock
{
    /// <summary>
    /// </summary>
    public AutoModeOff()
    {
        Cmd1 = Cmd1.TransportControl;
        Cmd2 = (byte)PresetSelectControl.PresetSelectControl.AutoModeOff;
    }
}