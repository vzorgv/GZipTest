namespace GZipTest.Processor
{
    using GZipTest.BlockGenerators;
    using GZipTest.Metadata;
    using GZipTest.TaskManagement;
    using GZipTest.Tasks;
    using System;
    using System.IO;

    internal sealed class Compressor : IArchiverProcessor
    {
        private readonly string _filenameToCompress;
        private readonly string _compressedFilename;
        private readonly int _blockSizeInBytes;

        private ThreadManager _threadManager = null;


        public Compressor(string filenameToCompress, string compressedFilename, int blockSizeInBytes)
        {
            _filenameToCompress = filenameToCompress;
            _compressedFilename = compressedFilename;
            _blockSizeInBytes = blockSizeInBytes;
        }


        public void StartProcess()
        {
            FileStream sourceFileStream = null;

            try
            {
                var initialWritePosition = Utils.CreateCompressedFileWithHeader(_compressedFilename);
                var fileMetadata = new CompressedFileMetadata();

                sourceFileStream = new FileStream(_filenameToCompress, FileMode.Open, FileAccess.Read, FileShare.Read);
                var compressTask = GetTask(sourceFileStream, _filenameToCompress, _compressedFilename, fileMetadata, _blockSizeInBytes, initialWritePosition);
                _threadManager = GetThreadManager(sourceFileStream, _blockSizeInBytes);

                sourceFileStream.Close();

                _threadManager.RunInParallel(compressTask);
                _threadManager.WaitAll();

                Utils.WriteMetadata(_compressedFilename, fileMetadata);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occured: {ex.Message}");
                Utils.DeleteFile(_compressedFilename);
                throw;
            }
            finally
            {
                sourceFileStream?.Dispose();
                _threadManager?.Dispose();
            }
        }

        public void StopProcess()
        {
            _threadManager?.StopAll();
        }

        private ICanceleableTask GetTask(Stream sourceFileStream, string fileNameToCompress, string compressedFileName, CompressedFileMetadata fileMetadata, int blockSize, long initialPosition)
        {
            var inputStreamPositionGenerator = new FixedSizeBlockGenerator(blockSize, sourceFileStream.Length);
            return new CompressTask(fileNameToCompress, compressedFileName, inputStreamPositionGenerator, fileMetadata, initialPosition);
        }

        private ThreadManager GetThreadManager(Stream sourceFileStream, int blockSizeInBytes)
        {
            var chunksCount = GetChunksCount(sourceFileStream.Length, blockSizeInBytes);
            return new ThreadManager(new LogicalProcPerChunkCountStrategy(chunksCount));
        }

        private int GetChunksCount(long totalSize, int blockSizeInBytes)
        {
            return (int)Math.Ceiling((double)totalSize / blockSizeInBytes);
        }
    }
}
