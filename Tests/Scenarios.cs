namespace Tests
{
    using GZipTest.BlockArchiver;
    using GZipTest.Metadata;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;

    [Category("File based")]
    [NonParallelizable]
    public class Scenarios
    {
        private static string toCompressFilename = Path.Combine(Path.GetTempPath(), "to_compress.txt");
        private static string compressedFilename = Path.Combine(Path.GetTempPath(), "compressed.gzt");
        private static string decompressedFilename = Path.Combine(Path.GetTempPath(), "de_compressed.txt");

        [OneTimeSetUp]
        public void Init()
        {
            using var toCompressFileStream = new FileStream(toCompressFilename, FileMode.Create);
            using var writer = new StreamWriter(toCompressFileStream);

            for (var i = 0; i < 22_000; i++)
            {
                writer.WriteLine($"{i}-th test line written");
            }
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
        }


        [Test]
        public void WhenCompress_DecompressFile_Checksum_TheSame()
        {
            var compressor = new Compressor(toCompressFilename, compressedFilename, 1024);
            compressor.Run();

            var decompressor = new Decompressor(compressedFilename, decompressedFilename);
            decompressor.Run();

            var expectedChecksum = CalcMD5Checksum(toCompressFilename);
            var actualChecksum = CalcMD5Checksum(decompressedFilename);

            Assert.True(Enumerable.SequenceEqual(expectedChecksum, actualChecksum), "Uncompressed file not equal to original file");
        }

        [Test]
        public void WhenFileNotExist_Compress_Should_ThrowException()
        {
            var compressor = new Compressor("_" + toCompressFilename, "_" + compressedFilename, 1024);
            var decompressor = new Decompressor(compressedFilename, "_" + decompressedFilename);

            Assert.Throws<IOException>(decompressor.Run);
        }

        [Test]
        public void WhenFileNotExist_Decompress_Should_ThrowException()
        {
            var compressor = new Compressor("_" + toCompressFilename, "_" + compressedFilename, 1024);

            Assert.Throws<IOException>(compressor.Run);
        }

        private byte[] CalcMD5Checksum(string filename)
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(filename);

            return md5.ComputeHash(stream);
        }
    }
}