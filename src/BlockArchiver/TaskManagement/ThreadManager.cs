namespace GZipTest.TaskManagement
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;

    internal sealed class ThreadManager : IDisposable
    {
        private const int LOOKUP_THREAD_STATE_TIMEOUT = 10;
        
        private readonly ConcurrentDictionary<int, Thread> _threads = new ConcurrentDictionary<int, Thread>();
        private readonly IThreadCountCalculationStrategy _threadCountCalculationStrategy;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private AutoResetEvent _autoResetEvent;

        private readonly object _errorSync = new object();
        private Exception _firstException = null;

        public ThreadManager(IThreadCountCalculationStrategy threadCountCalculationStrategy)
        {
            _threadCountCalculationStrategy = threadCountCalculationStrategy;
        }

        public void RunInParallel(ICanceleableTask runnable)
        {
            var threadCount = _threadCountCalculationStrategy.GetThreadCount();
            _autoResetEvent = new AutoResetEvent(false);


            for (int i = 0; i < threadCount; i++)
            {
                var thread = new Thread(new TaskRunWrapper(runnable, OnComplete, OnError).Run);
                _threads.TryAdd(thread.ManagedThreadId, thread);
                thread.Start(_cancellationTokenSource.Token);
            }
        }

        public void WaitAll()
        {
            while (_firstException == null && _threads.Count > 0)
                _autoResetEvent.WaitOne(LOOKUP_THREAD_STATE_TIMEOUT);

            if (_firstException != null)
                throw _firstException;
        }

        public void StopAll()
        {
            _cancellationTokenSource.Cancel();
            WaitAll();
        }

        private void OnError(Exception error)
        {
            _threads.TryRemove(Thread.CurrentThread.ManagedThreadId, out _);

            if (_firstException == null)
            {
                lock (_errorSync)
                {
                    if (_firstException == null)
                    {
                        _firstException = error;
                        _cancellationTokenSource.Cancel();
                        _autoResetEvent.Set();
                    }
                }
            }
        }

        private void OnComplete()
        {
            _threads.TryRemove(Thread.CurrentThread.ManagedThreadId, out _);
            _autoResetEvent.Set();
        }

        public void Dispose()
        {
            _autoResetEvent?.Dispose();
            _cancellationTokenSource.Dispose();
        }
    }
}
