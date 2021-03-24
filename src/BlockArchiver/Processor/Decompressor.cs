namespace GZipTest.Processor
{
    using GZipTest.TaskManagement;
    using GZipTest.Tasks;
    using System;

    internal sealed class Decompressor : IArchiverProcessor
    {
        private readonly string _compressedFilename;
        private readonly string _decompressedFilename;

        private ThreadManager _threadManager = null;

        public Decompressor(string compressedFilename, string decompressedFilename)
        {
            _compressedFilename = compressedFilename;
            _decompressedFilename = decompressedFilename;
        }

        public void StartProcess()
        {
            try
            {
                var decompressTask = GetTask();
                _threadManager = GetThreadManager();

                _threadManager.RunInParallel(decompressTask);
                _threadManager.WaitAll();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occured: {ex.Message}");
                Utils.DeleteFile(_compressedFilename);
                throw;
            }
            finally
            {
                _threadManager?.Dispose();
            }
        }

        public void StopProcess()
        {
            _threadManager?.StopAll();
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
