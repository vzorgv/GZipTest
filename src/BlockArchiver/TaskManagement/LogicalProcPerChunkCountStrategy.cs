namespace GZipTest
{
    using System;

    internal sealed class LogicalProcPerChunkCountStrategy : IThreadCountCalculationStrategy
    {
        private readonly int _countOfCunks;

        public LogicalProcPerChunkCountStrategy(int countOfCunks)
        {
            _countOfCunks = countOfCunks;
        }

        public int GetThreadCount()
        {
            if (_countOfCunks == 0)
            {
                return 0;
            }
            else
            {
                return Math.Min(_countOfCunks, Environment.ProcessorCount);
            }
        }
    }
}
