namespace GZipTest
{
    using GZipTest.BlockGenerators;
    using GZipTest.Metadata;
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using System.Text.Json;

    internal static class Utils
    {
        public static byte[] CompressBuffer(byte[] bufferToCompress)
        {
            using var strm = new MemoryStream();
            using var GZipStrem = new GZipStream(strm, CompressionMode.Compress);
            GZipStrem.Write(bufferToCompress, 0, bufferToCompress.Length);
            GZipStrem.Flush();
            strm.Flush();
            var compressedBuffer = strm.GetBuffer();

            return compressedBuffer;
        }

        public static long CreateCompressedFileWithHeader(string compressedFileName)
        {
            using FileStream outputStream = new FileStream(compressedFileName, FileMode.Create, FileAccess.Write);
            var metadataPositionPlaceholder = BitConverter.GetBytes((long)0);
            outputStream.Write(metadataPositionPlaceholder);

            return sizeof(long);
        }

        public static void WriteMetadata(string compressedFileName, CompressedFileMetadata fileMetadata)
        {
            using FileStream outputStream = new FileStream(compressedFileName, FileMode.Open, FileAccess.Write);

            var metadataPosition = BitConverter.GetBytes(outputStream.Length);
            outputStream.Write(metadataPosition);

            var serilizedData = JsonSerializer.Serialize(fileMetadata);

            using var memStream = new MemoryStream(Encoding.UTF8.GetBytes(serilizedData));
            using var zipStream = new GZipStream(outputStream, CompressionMode.Compress);

            outputStream.Seek(outputStream.Length, SeekOrigin.Begin);
            memStream.CopyTo(zipStream);
        }

        public static IBlockGenerator<BlockMetadata> DecompressMetadata(string compressedFileName)
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

        public static byte[] DecompressBlock(Stream compressedFileStream, BlockMetadata block)
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
