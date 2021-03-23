namespace GZipTest.TaskManagement
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;

    internal sealed class ThreadManager
    {
        private readonly ConcurrentDictionary<int, Thread> _threads = new ConcurrentDictionary<int, Thread>();
        private readonly IThreadCountCalculationStrategy _threadCountCalculationStrategy;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly object _errorSync = new object();

        private bool _inErrorState = false;

        public ThreadManager(IThreadCountCalculationStrategy threadCountCalculationStrategy)
        {
            _threadCountCalculationStrategy = threadCountCalculationStrategy;
        }

        public void RunInParallel(ICanceleableTask runnable)
        {
            var threadCount = _threadCountCalculationStrategy.GetThreadCount();

            for (int i = 0; i < threadCount; i++)
            {
                var thread = new Thread(new TaskRunWrapper(runnable, OnError).Run);
                _threads.TryAdd(thread.ManagedThreadId, thread);
                thread.Start(_cancellationTokenSource.Token);
            }
        }

        public void WaitAll()
        {
            foreach (var thread in _threads.Values)
            {
                thread.Join();
            }
        }

        public void StopAll()
        {
            _cancellationTokenSource.Cancel();

            WaitAll();
        }

        private void OnError(Exception error)
        {
            _threads.TryRemove(Thread.CurrentThread.ManagedThreadId, out _);

            if (!_inErrorState)
            {
                lock (_errorSync)
                {
                    if (!_inErrorState)
                    {
                        _inErrorState = true;
                        _cancellationTokenSource.Cancel();
                    }
                }
            }
        }
    }
}
