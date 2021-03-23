namespace GZipTest.Archiver
{
    using GZipTest.BlockGenerators;
    using GZipTest.Metadata;
    using GZipTest.TaskManagement;
    using GZipTest.Tasks;
    using System;
    using System.IO;
    using System.Threading;

    public class BlockArchiver
    {
        public void Compress(string fileNameToCompress, string compressedFileName, int blockSizeInBytes)
        {
            FileStream sourceFileStream = null;
            ThreadManager threadManager = null;

            try
            {
                var initialWritePosition = Utils.CreateCompressedFileWithHeader(compressedFileName);
                var fileMetadata = new CompressedFileMetadata();

                sourceFileStream = new FileStream(fileNameToCompress, FileMode.Open, FileAccess.Read, FileShare.Read);
                var compressTask = GetCompressTask(sourceFileStream, fileNameToCompress, compressedFileName, fileMetadata, blockSizeInBytes, initialWritePosition);
                threadManager = GetThreadManagerToCompress(sourceFileStream, blockSizeInBytes);

                sourceFileStream.Close();

                threadManager.RunInParallel(compressTask);
                threadManager.WaitAll();

                Utils.WriteMetadata(compressedFileName, fileMetadata);
            }
            finally
            {
                sourceFileStream?.Dispose();
                threadManager?.Dispose();
            }
        }

        private CompressTask GetCompressTask(Stream sourceFileStream, string fileNameToCompress, string compressedFileName, CompressedFileMetadata fileMetadata, int blockSize, long initialPosition)
        {
            var inputStreamPositionGenerator = new FixedSizeBlockGenerator(blockSize, sourceFileStream.Length);

            return new CompressTask(fileNameToCompress, compressedFileName, inputStreamPositionGenerator, fileMetadata, initialPosition);
        }

        private ThreadManager GetThreadManagerToCompress(Stream sourceFileStream, int blockSizeInBytes)
        {
            var chunksCount = GetChunksCount(sourceFileStream.Length, blockSizeInBytes);
            return new ThreadManager(new LogicalProcPerChunkCountStrategy(chunksCount));
        }

        private int GetChunksCount(long totalSize, int blockSizeInBytes)
        {
            return (int)Math.Ceiling((double)totalSize / blockSizeInBytes);
        }

        public void Decompress(string compressedFileName, string decompressedFileName)
        {
            var generator = Utils.DecompressMetadata(compressedFileName);
            var decompressTask = new DecompressTask(compressedFileName, decompressedFileName, generator);

            var cts = new CancellationTokenSource();
            decompressTask.Run(cts.Token);
        }
    }
}
