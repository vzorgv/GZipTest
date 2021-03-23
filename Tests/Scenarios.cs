namespace Tests
{
    using GZipTest.BlockArchiver;
    using NUnit.Framework;
    using System;
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

            for (var i = 0; i < 21_000; i++)
            {
                writer.WriteLine($"{i}-th test line written at {DateTime.Now}");
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

            Assert.True(Enumerable.SequenceEqual<byte>(expectedChecksum, actualChecksum), "Uncompressed file not equal to original file");
        }

        private byte[] CalcMD5Checksum(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    return md5.ComputeHash(stream);
                }
            }
        }
    }
}