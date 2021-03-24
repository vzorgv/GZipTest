namespace GZipTest.BlockArchiver
{
    using GZipTest.TaskManagement;
    using GZipTest.Tasks;
    using System;

    internal sealed class Decompressor
    {
        private readonly string _compressedFilename;
        private readonly string _decompressedFilename;

        public Decompressor(string compressedFilename, string decompressedFilename)
        {
            _compressedFilename = compressedFilename;
            _decompressedFilename = decompressedFilename;
        }

        public void Run()
        {
            ThreadManager threadManager = null;

            try
            {
                var decompressTask = GetTask();
                threadManager = GetThreadManager();

                threadManager.RunInParallel(decompressTask);
                threadManager.WaitAll();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occured: {ex.Message}");
                Utils.DeleteFile(_compressedFilename);
                throw;
            }
            finally
            {
                threadManager?.Dispose();
            }
        }

        private ICanceleableTask GetTask()
        {
            var generator = Utils.DecompressMetadata(_compressedFilename);
            return new DecompressTask(_compressedFilename, _decompressedFilename, generator);
        }

        private ThreadManager GetThreadManager()
        {
            return new ThreadManager(new LogicalProcPerChunkCountStrategy(1));
        }
    }
}
