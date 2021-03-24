namespace Tests
{
    using GZipTest;
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
            // Arrange
            const int OneKb = 1024;

            var compressorParms = new CreateProcessParms()
            {
                Operation = OperationType.Compress,
                SourceFile = toCompressFilename,
                DestinationFile = compressedFilename
            };

            var factory = new ProcessorFactory(compressorParms, OneKb);
            var compressor = factory.Create();

            var decompressorParms = new CreateProcessParms()
            {
                Operation = OperationType.Decompress,
                SourceFile = compressedFilename,
                DestinationFile = decompressedFilename
            };

            factory = new ProcessorFactory(decompressorParms, 0);
            var decompressor = factory.Create();

            // Act
            compressor.StartProcess();
            decompressor.StartProcess();

            // Assert
            var expectedChecksum = CalcMD5Checksum(toCompressFilename);
            var actualChecksum = CalcMD5Checksum(decompressedFilename);

            Assert.True(Enumerable.SequenceEqual(expectedChecksum, actualChecksum), "Uncompressed file not equal to original file");
        }

        [Test]
        public void WhenFileNotExist_Compress_Should_ThrowException()
        {
            // Arrange
            var compressor = new Compressor("_" + toCompressFilename, "_" + compressedFilename, 1024);

            Assert.Throws<IOException>(compressor.StartProcess);
        }

        [Test]
        public void WhenFileNotExist_Decompress_Should_ThrowException()
        {
            var decompressor = new Decompressor("_" + toCompressFilename, "_" + compressedFilename);

            Assert.Throws<IOException>(decompressor.StartProcess);
        }

        private byte[] CalcMD5Checksum(string filename)
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(filename);

            return md5.ComputeHash(stream);
        }
    }
}