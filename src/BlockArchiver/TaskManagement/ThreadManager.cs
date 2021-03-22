namespace GZipTest.TaskManagement
{
    using System.Collections.Generic;
    using System.Threading;

    internal sealed class ThreadManager
    {
        private readonly List<Thread> threads = new List<Thread>();
        private readonly IThreadCountCalculationStrategy _threadCountCalculationStrategy;

        public ThreadManager(IThreadCountCalculationStrategy threadCountCalculationStrategy)
        {
            _threadCountCalculationStrategy = threadCountCalculationStrategy;
        }

        public void Run(IRunnable runnable)
        {
            var threadCount = _threadCountCalculationStrategy.GetThreadCount();

            for (int i = 0; i < threadCount; i++)
            {
                var thread = new Thread(runnable.Run);
                threads.Add(thread);
                thread.Start();
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
            //TODO: implement
        }
    }
}
