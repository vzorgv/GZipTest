namespace GZipTest.BlockArchiver
{
    using GZipTest.BlockGenerators;
    using GZipTest.Metadata;
    using GZipTest.TaskManagement;
    using GZipTest.Tasks;
    using System;
    using System.IO;

    internal sealed class Compressor
    {
        private readonly string _filenameToCompress;
        private readonly string _compressedFilename;
        private readonly int _blockSizeInBytes;

        public Compressor(string filenameToCompress, string compressedFilename, int blockSizeInBytes)
        {
            _filenameToCompress = filenameToCompress;
            _compressedFilename = compressedFilename;
            _blockSizeInBytes = blockSizeInBytes;
        }

        public void Run()
        {
            FileStream sourceFileStream = null;
            ThreadManager threadManager = null;

            try
            {
                var initialWritePosition = Utils.CreateCompressedFileWithHeader(_compressedFilename);
                var fileMetadata = new CompressedFileMetadata();

                sourceFileStream = new FileStream(_filenameToCompress, FileMode.Open, FileAccess.Read, FileShare.Read);
                var compressTask = GetTask(sourceFileStream, _filenameToCompress, _compressedFilename, fileMetadata, _blockSizeInBytes, initialWritePosition);
                threadManager = GetThreadManager(sourceFileStream, _blockSizeInBytes);

                sourceFileStream.Close();

                threadManager.RunInParallel(compressTask);
                threadManager.WaitAll();

                Utils.WriteMetadata(_compressedFilename, fileMetadata);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occured: {ex.Message}");
                throw;
            }
            finally
            {
                sourceFileStream?.Dispose();
                threadManager?.Dispose();
                Utils.DeleteFile(_compressedFilename);
            }
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
