namespace Tests
{
    using GZipTest.Processor;
    using NUnit.Framework;
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

            // Generate file ~1Mb
            for (var i = 0; i < 22_000; i++)
            {
                writer.WriteLine($"{i} - th test line written");
            }
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            File.Delete(compressedFilename);
            File.Delete(toCompressFilename);
            File.Delete(decompressedFilename);
        }


        [Test]
        public void WhenCompress_DecompressFile_Checksums_TheSame()
        {
            const int OneKb = 1024;

            var compressor = new Compressor(toCompressFilename, compressedFilename, OneKb);
            compressor.StartProcess();

            var decompressor = new Decompressor(compressedFilename, decompressedFilename);
            decompressor.StartProcess();

            var expectedChecksum = CalcMD5Checksum(toCompressFilename);
            var actualChecksum = CalcMD5Checksum(decompressedFilename);

            Assert.True(Enumerable.SequenceEqual(expectedChecksum, actualChecksum), "Uncompressed file not equal to original file");
        }

        [Test]
        public void WhenFileNotExist_Compress_Should_ThrowException()
        {
            var compressor = new Compressor("_" + toCompressFilename, "_" + compressedFilename, 1024);
            var decompressor = new Decompressor(compressedFilename, "_" + decompressedFilename);

            Assert.Throws<IOException>(decompressor.StartProcess);
        }

        [Test]
        public void WhenFileNotExist_Decompress_Should_ThrowException()
        {
            var compressor = new Compressor("_" + toCompressFilename, "_" + compressedFilename, 1024);

            Assert.Throws<IOException>(compressor.StartProcess);
        }

        private byte[] CalcMD5Checksum(string filename)
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(filename);

            return md5.ComputeHash(stream);
        }
    }
}