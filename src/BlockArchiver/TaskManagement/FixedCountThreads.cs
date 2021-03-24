using System;

namespace GZipTest.TaskManagement
{
    internal sealed class FixedCountThreads : IThreadCountCalculationStrategy
    {
         public int GetThreadCount()
        {
            return Environment.ProcessorCount;
        }
    }
}
