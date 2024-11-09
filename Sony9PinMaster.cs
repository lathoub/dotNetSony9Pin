﻿using lathoub.dotNetSony9Pin.Pattern;
using lathoub.dotNetSony9Pin.Sony9Pin.CommandBlocks;
using lathoub.dotNetSony9Pin.Sony9Pin.CommandBlocks.Return;
using lathoub.dotNetSony9Pin.Sony9Pin.CommandBlocks.SenseReturn;
using lathoub.dotNetSony9Pin.Sony9Pin.CommandBlocks.StatusData;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;

namespace lathoub.dotNetSony9Pin;

internal class Sony9PinMaster : Sony9PinBase
{
    public string Model { get; internal set; } = "";

    private StatusData _statusData = new();

    private TimeCode _timeCode = new();

    public StatusData StatusData
    {
        get => _statusData;

        set
        {
            if (!_statusData.Equals(value))
            {
                RaiseStatusDataChangingHandler(_statusData);
                lock (this)
                {
                    _statusData = value;
                }
                RaiseStatusDataChangedHandler(_statusData);
            }
            RaiseStatusDataReceivedHandler(_statusData);
        }
    }

    public TimeCode TimeCode
    {
        get => _timeCode;
    }

    #region Events and EventHandlers

    /// <summary>
    /// 
    /// </summary>
    public event EventHandler<DeviceTypeEventArgs>? DeviceType;

    /// <summary>
    /// 
    /// </summary>
    public event EventHandler<ConnectedEventArgs>? ConnectedChanged;

    /// <summary>
    /// 
    /// </summary>
    public event EventHandler? Nak;

    /// <summary>
    /// 
    /// </summary>
    public event EventHandler<StatusDataEventArgs>? StatusDataReceived;

    /// <summary>
    ///     The status data.
    /// </summary>
    public event EventHandler<StatusDataEventArgs>? StatusDataChanged;

    /// <summary>
    ///     The status data.
    /// </summary>
    public event EventHandler<StatusDataEventArgs>? StatusDataChanging;

    /// <summary>
    ///     The time data.
    /// </summary>
    public event EventHandler<TimeDataEventArgs>? TimeDataReceived;

    /// <summary>
    ///     The time data.
    /// </summary>
    public event EventHandler<TimeDataEventArgs>? TimeDataChanged;

    /// <summary>
    ///     The time data.
    /// </summary>
    public event EventHandler<TimeDataEventArgs>? TimeDataChanging;

    /// <summary>
    ///     The raise device type handler.
    /// </summary>
    /// <param name="deviceName">
    ///     The sender.
    /// </param>
    protected virtual void RaiseDeviceTypeHandler(string deviceName)
    {
        var handler = DeviceType;
        handler?.Invoke(this, new DeviceTypeEventArgs(deviceName));
    }

    protected virtual void RaiseConnectedChanged(bool connected)
    {
        var handler = ConnectedChanged;
        handler?.Invoke(this, new ConnectedEventArgs(connected));
    }

    protected virtual void RaiseNakHandler(NakCommandBlock.Nak error)
    {
        var handler = Nak;
        handler?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    ///     The raise status data handler.
    /// </summary>
    /// <param name="statusData">
    ///     The sender.
    /// </param>
    protected virtual void RaiseStatusDataChangedHandler(StatusData statusData)
    {
        var handler = StatusDataChanged;
        handler?.Invoke(this, new StatusDataEventArgs(statusData));
    }

    /// <summary>
    ///     The raise status data handler.
    /// </summary>
    /// <param name="statusData">
    ///     The sender.
    /// </param>
    protected virtual void RaiseStatusDataChangingHandler(StatusData statusData)
    {
        var handler = StatusDataChanging;
        handler?.Invoke(this, new StatusDataEventArgs(statusData));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="statusData"></param>
    protected virtual void RaiseStatusDataReceivedHandler(StatusData statusData)
    {
        var handler = StatusDataReceived;
        handler?.Invoke(this, new StatusDataEventArgs(statusData));
    }

    /// <summary>
    ///     The raise time data handler.
    /// </summary>
    /// <param name="senseReturn">
    ///     The sender.
    /// </param>
    /// <param name="timeCode">
    ///     The sender.
    /// </param>
    protected virtual void RaiseTimeDataReceivedHandler(SenseReturn senseReturn, TimeCode timeCode)
    {
        var handler = TimeDataReceived;
        handler?.Invoke(this, new TimeDataEventArgs(senseReturn, timeCode));
    }

    /// <summary>
    ///     The raise time data handler.
    /// </summary>
    /// <param name="senseReturn">
    ///     The sender.
    /// </param>
    /// <param name="timeCode">
    ///     The sender.
    /// </param>
    protected virtual void RaiseTimeDataChangedHandler(SenseReturn senseReturn, TimeCode timeCode)
    {
        var handler = TimeDataChanged;
        handler?.Invoke(this, new TimeDataEventArgs(senseReturn, timeCode));
    }

    /// <summary>
    ///     The raise time data handler.
    /// </summary>
    /// <param name="senseReturn">
    ///     The sender.
    /// </param>
    /// <param name="timeCode">
    ///     The sender.
    /// </param>
    protected virtual void RaiseTimeDataChangingHandler(SenseReturn senseReturn, TimeCode timeCode)
    {
        var handler = TimeDataChanging;
        handler?.Invoke(this, new TimeDataEventArgs(senseReturn, timeCode));
    }

    #endregion

    private readonly AutoResetEvent _workerThreadStopped = new(false);

    private readonly BackgroundWorker _serialReaderWorker = new() { WorkerReportsProgress = false, WorkerSupportsCancellation = true };
    private readonly BackgroundWorker _idleWorker = new() { WorkerReportsProgress = false, WorkerSupportsCancellation = true };

    private AutoResetEvent _requestReady = new(false);

    private AutoResetEvent _fireIdleCommand = new(false);

    private System.Timers.Timer _idleTimer = new();

    private readonly object _lock = new();

    private bool _connected;

    /// <summary>
    ///     Gets or sets the port name.
    /// </summary>
    public bool IsConnected
    {
        get => _connected;

        set
        {
            if (value == _connected) return;

            _connected = value;
            RaiseConnectedChanged(_connected);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public Sony9PinMaster()
    {
        _serialReaderWorker.DoWork += SerialReader;
        _idleWorker.DoWork += IdleReader;

        _idleTimer.Interval = 5; // Idle timeout is 5ms
        _idleTimer.Elapsed += (sender, e) => _fireIdleCommand.Set();
        _idleTimer.AutoReset = true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="port"></param>
    /// <returns></returns>
    public override bool Connect(string port)
    {
        if (!base.Connect(port))
            return false;

        _serialReaderWorker.RunWorkerAsync();
        _idleWorker.RunWorkerAsync();

        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override void Disconnect()
    {

        // Let the this._queueReaderWorker know its time to finish
        // Doesn't matter is the tread is busy or not (ég is Open was not successful, the
        // QueueReaderWorker is not started and IsBusy will be false)
        if (_serialReaderWorker is { WorkerSupportsCancellation: true })
            _serialReaderWorker.CancelAsync();
        if (_idleWorker is { WorkerSupportsCancellation: true })
            _idleWorker.CancelAsync();

        // Wait here for worker thread to signal its done
        if (_serialReaderWorker.IsBusy || _idleWorker.IsBusy)
            if (!_workerThreadStopped.WaitOne()) // will block until _workerThreadStopped.Set() call made at end of worker thread
                Debug.WriteLine("Could not stop serial master thread");

        base.Disconnect();
    }

    /// <summary>
    /// </summary>
    /// <param name="res">
    ///     The response.
    /// </param>
    public void ProcessResponse(CommandBlock res)
    {
        if (null == res)
            return;

        //Trace.WriteLineIf(TraceSwitch.TraceVerbose, "Response: " + response);

        switch (res.Cmd1)
        {
            case Cmd1.Return:
                switch ((Return)res.Cmd2)
                {
                    case Return.Ack:
                        // Indication that last Cmd1 was alright
                        //RaiseAckHandler();
                        break;

                    case Return.Nak:
                        //The master must immediatly stop sending data when it receives a NAK +Error Data message.
                        //this.Enabled = false;

                        var bits = new BitArray(new int[] { res.Data[0] });
                        //if (bits.Get((int)NakCommandBlock.Nak.ChecksumError))
                        //{
                        //    Thread.Sleep(10);
                        //}
                        //if (bits.Get((int)NakCommandBlock.Nak.FrameError))
                        //{
                        //    Thread.Sleep(10);
                        //}
                        //if (bits.Get((int)NakCommandBlock.Nak.OverrunError))
                        //{
                        //    Thread.Sleep(10);
                        //}
                        //if (bits.Get((int)NakCommandBlock.Nak.ParityError))
                        //{
                        //    Thread.Sleep(10);
                        //}
                        //if (bits.Get((int)NakCommandBlock.Nak.TimeOut))
                        //{
                        //    Thread.Sleep(10);
                        //}
                        //if (bits.Get((int)NakCommandBlock.Nak.UndefinedError))
                        //{
                        //}
                        RaiseNakHandler((NakCommandBlock.Nak)res.Data[0]);
                        break;

                    case Return.DeviceType:
                        {
                            var deviceId = (ushort)(res.Data[0] << 8 | res.Data[1]);
                            if (!Device.Names.TryGetValue(deviceId, out string? deviceName))
                                deviceName = BitConverter.ToString(res.Data).Replace("-", string.Empty);

                            Model = deviceName ?? "Unknown";

                            RaiseDeviceTypeHandler(Model);
                        }
                        break;
                }

                break;

            case Cmd1.SenseReturn:
                switch ((SenseReturn)res.Cmd2)
                {
                    case SenseReturn.Timer1Data:
                    case SenseReturn.Timer2Data:
                    case SenseReturn.LtcTimeData:
                    case SenseReturn.UserBitsLtcData:
                    case SenseReturn.VitcTimeData:
                    case SenseReturn.UserBitsVitcData:
                    case SenseReturn.GenTimeData:
                    case SenseReturn.GenUserBitsData:
                    case SenseReturn.CorrectedLtcTimeData:
                    case SenseReturn.HoldUbLtcData:
                    case SenseReturn.HoldVitcTimeData:
                    case SenseReturn.HoldUbVitcData:
                        var timeCode = new TimeCode(res.Data);
                        if (!timeCode.Equals(_timeCode))
                        {
                            RaiseTimeDataChangingHandler((SenseReturn)res.Cmd2, _timeCode);
                            lock (this)
                            {
                                _timeCode = timeCode;
                            }
                            RaiseTimeDataChangedHandler((SenseReturn)res.Cmd2, _timeCode);
                        }
                        RaiseTimeDataReceivedHandler((SenseReturn)res.Cmd2, _timeCode);
                        break;

                    case SenseReturn.StatusData:
                        StatusData = new StatusData(res.Data);
                        break;
                }

                break;
        }
    }

    public string ParseResponse(CommandBlock res)
    {
        if (null == res)
            return "";

        switch (res.Cmd1)
        {
            case Cmd1.Return:
                switch ((Return)res.Cmd2)
                {
                    case Return.Ack:
                        // Indication that last Cmd1 was alright
                        //RaiseAckHandler();
                        return "Ack";
                        break;

                    case Return.Nak:
                        //The master must immediatly stop sending data when it receives a NAK +Error Data message.
                        //this.Enabled = false;

                        var bits = new BitArray(new int[] { res.Data[0] });
                        //if (bits.Get((int)NakCommandBlock.Nak.ChecksumError))
                        //{
                        //    Thread.Sleep(10);
                        //}
                        //if (bits.Get((int)NakCommandBlock.Nak.FrameError))
                        //{
                        //    Thread.Sleep(10);
                        //}
                        //if (bits.Get((int)NakCommandBlock.Nak.OverrunError))
                        //{
                        //    Thread.Sleep(10);
                        //}
                        //if (bits.Get((int)NakCommandBlock.Nak.ParityError))
                        //{
                        //    Thread.Sleep(10);
                        //}
                        //if (bits.Get((int)NakCommandBlock.Nak.TimeOut))
                        //{
                        //    Thread.Sleep(10);
                        //}
                        //if (bits.Get((int)NakCommandBlock.Nak.UndefinedError))
                        //{
                        //}
                        //RaiseNakHandler((NakCommandBlock.Nak)res.Data[0]);
                        return bits.ToString();
                        break;

                    case Return.DeviceType:
                        {
                            var deviceId = (ushort)(res.Data[0] << 8 | res.Data[1]);
                            if (!Device.Names.TryGetValue(deviceId, out string? deviceName))
                                deviceName = BitConverter.ToString(res.Data).Replace("-", string.Empty);

                            return deviceName ?? "Unknown";
                        }
                        break;
                }

                break;

            case Cmd1.SenseReturn:
                switch ((SenseReturn)res.Cmd2)
                {
                    case SenseReturn.Timer1Data:
                    case SenseReturn.Timer2Data:
                    case SenseReturn.LtcTimeData:
                    case SenseReturn.UserBitsLtcData:
                    case SenseReturn.VitcTimeData:
                    case SenseReturn.UserBitsVitcData:
                    case SenseReturn.GenTimeData:
                    case SenseReturn.GenUserBitsData:
                    case SenseReturn.CorrectedLtcTimeData:
                    case SenseReturn.HoldUbLtcData:
                    case SenseReturn.HoldVitcTimeData:
                    case SenseReturn.HoldUbVitcData:
                        var timeCode = new TimeCode(res.Data);
                        return timeCode.ToString();
                        break;

                    case SenseReturn.StatusData:
                        var statusData = new StatusData(res.Data);
                        return statusData.ToString();
                        break;
                }

                break;
        }

        return "unhandled";
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="ArgumentNullException"></exception>
    private void SerialReader(object? sender, DoWorkEventArgs e)
    {
        Debug.WriteLine("Sony9PinMaster Starting QueueReader");

        if (sender is not BackgroundWorker worker)
            throw new ArgumentNullException(nameof(sender));

        // Make sure we have an empty buffer
        List<byte>? InputBuffer = new(32);

        while (!worker.CancellationPending)
        {
            if (!SerialPort.IsOpen)
                break;

            //
            if (!_requestReady.WaitOne(1))
                continue;

            // Make sure we have an empty buffer
            InputBuffer.Clear();

            try
            {
                Stopwatch stopwatch = new();
                stopwatch.Start();

                // Read characters from the serialPort until we can create a 
                // complete and valid CommandBlock
                while (SerialPort.IsOpen)
                {
                    var b = SerialPort.ReadByte();
                    if (b == -1)
                        break; // No more data to read
                    InputBuffer.Add((byte)b);

                    if (stopwatch.ElapsedMilliseconds > 9)
                        throw new TimeoutException($"Response took over 9ms. {stopwatch.ElapsedMilliseconds}");

                    if (!CommandBlock.TryParse(InputBuffer, out CommandBlock? res))
                        continue;
                    if (null == res) break;

                    // OK, we have enough characters for a valid CommandBlock.
                    stopwatch.Stop();
                    Debug.WriteLine($"Response only in: {stopwatch.ElapsedMilliseconds} ms");
                    Debug.WriteLine($"serial bytes remaining {SerialPort.BytesToRead}");
                    Debug.Assert(0 == SerialPort.BytesToRead, "serial bytes remaining is not zero");

                    IsConnected = true;

                    Received(res);

                    // We are done here, break back into the main loop to 
                    // try to take another CommandBlack
                    break;
                }
            }
            catch (TimeoutException ex)
            {
                // Oei - a character couldn't be read within the time given.
                Debug.WriteLine(ex.Message);

                IsConnected = false;

                Received(new CommandBlock()); // return error object
            }
            catch (Exception)
            {
                break; // Leave after this error
            }
            finally
            {
                _idleTimer.Start();
            }
        }

        e.Cancel = true;

        _workerThreadStopped.Set(); // signal that worker is done

        // Indicate here that we have been disconnected
        IsConnected = false;

        Debug.WriteLine("Sony9PinMaster Stopped QueueReader");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="ArgumentNullException"></exception>
    private void IdleReader(object? sender, DoWorkEventArgs e)
    {
        Debug.WriteLine("Sony9PinMaster Starting IdleReader");

        int _currentTimeSenseOrStatusSense = 0;

        if (sender is not BackgroundWorker worker)
            throw new ArgumentNullException(nameof(sender));

        while (!worker.CancellationPending)
        {
            // After last good CommandBlock is received, await for idle.
            // Then send idle command

            if (!_fireIdleCommand.WaitOne(1))
                continue;

            //if (_currentTimeSenseOrStatusSense == 0)
            //{
            //    var r = this.SendAsync(new StatusSense()).Result;
            //    Debug.WriteLine(ParseResponse(r));
            //    Debug.WriteLine($"==============================================================");
            //}
            //else if (_currentTimeSenseOrStatusSense == 1)
            //{
            //    var r = this.SendAsync(new CurrentTimeSense(TimeSenseRequest.LtcTime)).Result;
            //    Debug.WriteLine(ParseResponse(r));
            //    Debug.WriteLine($"==============================================================");
            //}

            _currentTimeSenseOrStatusSense++;
            if (_currentTimeSenseOrStatusSense > 1)
                _currentTimeSenseOrStatusSense = 0;
        }

        Debug.WriteLine("Sony9PinMaster Stopped IdleReader");
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void Send(CommandBlock req)
    {
        Debug.WriteLine($"Send: {req}");

        _idleTimer.Stop();
        _requestReady.Set();

        var bytes = req.ToBytes();
        SerialPort.Write(bytes, 0, bytes.Length);
    }

}
