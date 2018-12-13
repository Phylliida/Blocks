using System;
using System.Collections.Generic;
using System.Threading;

namespace P2P
{
    /// <summary>
    /// Because apple dispatch queue is amazing and I miss it
    /// This creates a thread assigned to it.
    /// When you call 
    /// DispatchQueue queue = new DispatchQueue();
    /// queue.async(() => { 
    /// // (some code)
    /// });
    /// That code is guaranteed to be ran on the thread assigned to that queue.
    /// This means that you can assume that anything you passed into it will be called in a "single threaded" manner,
    /// since there will be no race conditions between two functions passed into async.
    /// Make sure to watch for race conditions between stuff you pass into a DispatchQueue and stuff that you execute elsewhere though.
    /// This allows you to work with multithreading without usually having to bother with locks and stuff
    /// Please remember to call .Dispose() when you are done.
    /// Btw, it is safe to call async within an async callback.
    /// </summary>
    public class DispatchQueue : IDisposable
    {
        CancellationTokenSource source;
        public CancellationToken cancellationToken;
        Queue<QueueTask> tasks = new Queue<QueueTask>();
        ManualResetEvent moreStuffReceived;
        bool done = false;
        public DispatchQueue()
        {
            // true means that the door is "open"
            // false means that the door is "closed"
            // calling Set() will set the door to "open". If the door is already open, this does nothing.
            // WaitOne() will block until the door is set to "open" by someone else (on another thread)
            moreStuffReceived = new ManualResetEvent(false);
            Thread t = new Thread(() =>
            {
                using(source = new CancellationTokenSource()) // TODO: this could cause threading issues if something else is using this token when it is disposed
                {
                    cancellationToken = source.Token;
                    while (!done)
                    {
                        QueueTask task = null;
                        lock (tasks)
                        {
                            if (tasks.Count > 0)
                            {
                                task = tasks.Dequeue();
                            }
                            else if (!done)
                            {
                                moreStuffReceived.WaitOne();
                            }
                        }
                        if (task != null)
                        {
                            try
                            {
                                task();
                            }
                            catch (Exception e)
                            {
                                UnityEngine.Debug.LogError(e);
                            }
                        }
                    }
                    moreStuffReceived.Dispose();
                    source.Cancel();
                }
            });

            t.Start();
        }

        public delegate void QueueTask();

        /// <summary>
        /// Queues a task to be executed eventually.
        /// Dispatch Queues have a thread assigned to them.
        /// All tasks queued this way are guaranteed to be executed in the order queued, on that thread.
        /// </summary>
        /// <param name="task"></param>
        public void async(QueueTask task)
        {
            moreStuffReceived.Set();
            lock (tasks)
            {
                tasks.Enqueue(task);
                moreStuffReceived.Set();
            }
        }

        public void sync(QueueTask task)
        {
            task();
        }

        public void Dispose()
        {
            done = true;
            moreStuffReceived.Set();
            lock (tasks)
            {
                moreStuffReceived.Set();
            }
        }
    }
}