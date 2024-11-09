﻿using System.Diagnostics;

namespace lathoub;

public abstract class RequestResponsePump<T, U> 
{
    #region Events

    private event EventHandler<U>? ReceivedResponse;

    private void RaiseResponse(U response)
    {
        var handler = ReceivedResponse;
        handler?.Invoke(this, response);
    }

    #endregion

    private readonly object _lock = new();

    /// <summary>
    /// Only Async Send method
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    public virtual Task<U>? SendAsync(T req)
    {
        lock (_lock)
        {
            Debug.WriteLine($"--------------------------------------------------------------");
            Debug.WriteLine($"SendAsync: {req}");

            var promise = new TaskCompletionSource<U>();

            void handler(object? sender, U e)
            {
                ReceivedResponse -= handler;
                promise.TrySetResult(e);
            }
            ReceivedResponse += handler;

            Send(req);

            var completed = Task.WhenAny(promise.Task, Task.Delay(500));
            if (completed.Result != promise.Task)
            {
                promise.SetCanceled();

                throw new Exception("Timeout");
            }

            return promise.Task;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    protected abstract void Send(T req);

    /// <summary>
    /// 
    /// </summary>
    protected virtual void Received(U res)
    {
        Debug.WriteLine($"Response: {res}");

        RaiseResponse(res);
    }

}
