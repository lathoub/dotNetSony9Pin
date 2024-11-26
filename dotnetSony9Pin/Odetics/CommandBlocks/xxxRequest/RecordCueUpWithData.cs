using System.Diagnostics;
using dotNetSony9Pin.Sony9Pin.CommandBlocks;

namespace dotNetSony9Pin.Odetics.CommandBlocks.xxxRequest;

public class RecordCueUpWithData : CommandBlock
{
    /// <summary>
    /// 
    /// </summary>
    public RecordCueUpWithData()
    {
        Cmd1 = CommandFunction.xxxRequest;
        Cmd2 = (byte)xxxRequest.RecordCueUpWithData;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tc"></param>
    public RecordCueUpWithData(TimeCode tc)
    {
        var data = tc.ToBinaryCodedDecimal();

        Cmd1 = CommandFunction.xxxRequest;
        Cmd2 = (byte)xxxRequest.RecordCueUpWithData;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    public RecordCueUpWithData(byte[] id)
    {
        Cmd1 = CommandFunction.xxxRequest;
        Cmd2 = (byte)xxxRequest.RecordCueUpWithData;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tc"></param>
    /// <param name="id"></param>
    public RecordCueUpWithData(TimeCode tc, byte[] id)
    {
        Debug.Assert(id != null);
        if (id.Length != 8)
            throw new ArgumentException($"id must be exactly 8 bytes. Id is {id.Length} bytes");

        var buffer = new byte[13];

        Buffer.BlockCopy(id, 0, buffer, 0, id.Length);
        Buffer.BlockCopy(tc.ToBinaryCodedDecimal(), 0, buffer, id.Length, 4);

        Cmd1DataCount = ToCmd1DataCount(CommandFunction.xxxRequest, buffer.Length);
        Cmd2 = (byte)xxxRequest.RecordCueUpWithData;
        Data = buffer;
    }

}