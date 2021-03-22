namespace GZipTest
{
    using GZipTest.Metadata;
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using System.Text.Json;

    internal static class Utils
    {
        public static  byte[] CompressBuffer(byte[] bufferToCompress)
        {
            using var strm = new MemoryStream();
            using var GZipStrem = new GZipStream(strm, CompressionMode.Compress);
            GZipStrem.Write(bufferToCompress, 0, bufferToCompress.Length);
            GZipStrem.Flush();
            strm.Flush();
            var compressedBuffer = strm.GetBuffer();

            return compressedBuffer;
        }

        public static void CreateCompressedFile(string compressedFileName)
        {
            using FileStream outputStream = new FileStream(compressedFileName, FileMode.Create, FileAccess.Write);
            var metadataPositionPlaceholder = BitConverter.GetBytes((long)0);
            outputStream.Write(metadataPositionPlaceholder);
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
    }
}
