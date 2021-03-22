namespace GZipTest
{
    using GZipTest.Metadata;
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Text;

    public class BlockArchiver
    {
        public void Compress(string fileNameToCompress, string compressedFileName, int blockSizeInBytes)
        {
            FileStream sourceFileStream = null;

            try
            {
                sourceFileStream = new FileStream(fileNameToCompress, FileMode.Open, FileAccess.Read, FileShare.Read);

                var fileMetadata = new CompressedFileMetadata();

                var compressTask = GetCompressTask(sourceFileStream, fileNameToCompress, compressedFileName, fileMetadata);
                var threadManager = GetThreadManagerToCompress(sourceFileStream, blockSizeInBytes);

                Utils.CreateCompressedFile(compressedFileName);

                threadManager.Run(compressTask);
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

        private CompressTask GetCompressTask(Stream sourceFileStream, string fileNameToCompress, string compressedFileName, CompressedFileMetadata fileMetadata)
        {
            var inputStreamPositionGenerator = new StreamPositionGenerator(0, sourceFileStream.Length);
            var outputStreamPositionGenerator = new StreamPositionGenerator(0);

            return new CompressTask(fileNameToCompress, compressedFileName, inputStreamPositionGenerator, outputStreamPositionGenerator, fileMetadata);
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
            var generator = DecompressMetadata(compressedFileName);

            using var compressedFileStream = new FileStream(compressedFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var decompressedFileStream = new FileStream(decompressedFileName, FileMode.Create, FileAccess.Write);

            BlockMetadata block = null;

            while (generator.TryGetNext(0, out block))
            {
                compressedFileStream.Seek(block.CompressedPosition, SeekOrigin.Begin);
                var decompressedData = DecompressBlock(compressedFileStream, block);

                decompressedFileStream.Seek(block.OriginalPosition, SeekOrigin.Begin);
                var x = Encoding.UTF8.GetString(decompressedData);
                decompressedFileStream.Write(decompressedData, 0, decompressedData.Length);
            }
        }

        private IBlockGenerator<BlockMetadata> DecompressMetadata(string compressedFileName)
        {
            using var compressedFileStream = new FileStream(compressedFileName, FileMode.Open, FileAccess.Read);
            byte[] metadataPositionBytes = new byte[sizeof(long)];

            compressedFileStream.Read(metadataPositionBytes, 0, metadataPositionBytes.Length);
            var metadataPosition = BitConverter.ToInt64(metadataPositionBytes);

            compressedFileStream.Seek(metadataPosition, SeekOrigin.Begin);

            using var decompressedMemoryStream = new MemoryStream();
            using var decompressionStream = new GZipStream(compressedFileStream, CompressionMode.Decompress);
            decompressionStream.CopyTo(decompressedMemoryStream);

            decompressedMemoryStream.Position = 0;

            return new BlockMetadataGenerator(decompressedMemoryStream);
        }

        private byte[] DecompressBlock(Stream compressedFileStream, BlockMetadata block)
        {
            var compressedBuffer = new byte[block.SizeInBytes];
            compressedFileStream.Read(compressedBuffer, 0, compressedBuffer.Length);
            var compressedStream = new MemoryStream(compressedBuffer);

            using var decompressedMemoryStream = new MemoryStream();
            using var decompressionStream = new GZipStream(compressedStream, CompressionMode.Decompress);
            decompressionStream.CopyTo(decompressedMemoryStream);

            return decompressedMemoryStream.ToArray();
        }
    }
}
