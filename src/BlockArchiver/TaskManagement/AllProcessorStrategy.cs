namespace GZipTest.TaskManagement
{
    using System;

    internal sealed class AllProcessorStrategy : IThreadCountCalculationStrategy
    {
        public int GetThreadCount()
        {
            return Environment.ProcessorCount;
        }
    }
}
