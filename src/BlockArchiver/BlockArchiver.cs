namespace GZipTest.Archiver
{
    using GZipTest.BlockGenerators;
    using GZipTest.Metadata;
    using GZipTest.TaskManagement;
    using GZipTest.Tasks;
    using System;
    using System.IO;

    //TODO:  construct tasks and task manager using factory
    public class BlockArchiver
    {
        public void Compress(string fileNameToCompress, string compressedFileName, int blockSizeInBytes)
        {
            FileStream sourceFileStream = null;

            try
            {
                sourceFileStream = new FileStream(fileNameToCompress, FileMode.Open, FileAccess.Read, FileShare.Read);
                var fileMetadata = new CompressedFileMetadata();

                var compressTask = GetCompressTask(sourceFileStream, fileNameToCompress, compressedFileName, fileMetadata, blockSizeInBytes);
                var threadManager = GetThreadManagerToCompress(sourceFileStream, blockSizeInBytes);

                Utils.CreateCompressedFile(compressedFileName);

                threadManager.RunInParallel(compressTask);
                threadManager.WaitAll();

                Utils.WriteMetadata(compressedFileName, fileMetadata);
            }
            finally
            {
                if (sourceFileStream != null)
                {
                    sourceFileStream.Dispose();
                }
            }
        }

        private CompressTask GetCompressTask(Stream sourceFileStream, string fileNameToCompress, string compressedFileName, CompressedFileMetadata fileMetadata, int blockSize)
        {
            var inputStreamPositionGenerator = new FixedSizeBlockGenerator(blockSize, sourceFileStream.Length);

            return new CompressTask(fileNameToCompress, compressedFileName, inputStreamPositionGenerator, fileMetadata);
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
            decompressTask.Run();
        }
    }
}
