namespace GZipTest.TaskManagement
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal sealed class ThreadManager
    {
        private readonly List<Thread> threads = new List<Thread>();
        private readonly IThreadCountCalculationStrategy _threadCountCalculationStrategy;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public ThreadManager(IThreadCountCalculationStrategy threadCountCalculationStrategy)
        {
            _threadCountCalculationStrategy = threadCountCalculationStrategy;
        }

        public void RunInParallel(ICanceleableTask runnable)
        {
            var threadCount = _threadCountCalculationStrategy.GetThreadCount();

            for (int i = 0; i < threadCount; i++)
            {
                var thread = new Thread(new TaskRunWrapper(runnable).Run);
                threads.Add(thread);
                thread.Start(_cancellationTokenSource.Token);
            }
        }

        public void WaitAll()
        {
            foreach (var thread in threads)
            {
                thread.Join();
            }
        }

        public void StopAll()
        {
            _cancellationTokenSource.Cancel();

            WaitAll();
        }
    }
}
