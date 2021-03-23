namespace GZipTest.Tasks
{
    using GZipTest.BlockGenerators;
    using GZipTest.Metadata;
    using GZipTest.TaskManagement;
    using System.IO;

    internal sealed class DecompressTask : IRunnable
    {
        private readonly IBlockGenerator<BlockMetadata> _blockGenerator;
        private readonly string _decompressedFileName;
        private readonly string _compressedFilename;

        public DecompressTask(string compressedFilename, string decompressedFileName, IBlockGenerator<BlockMetadata> blockGenerator)
        {
            _compressedFilename = compressedFilename;
            _decompressedFileName = decompressedFileName;
            _blockGenerator = blockGenerator;
        }

        public void Run()
        {
            //TODO: implement gracefully shutdown
            Decompress();
        }

        private void Decompress()
        {
            using var compressedFileStream = new FileStream(_compressedFilename, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var decompressedFileStream = new FileStream(_decompressedFileName, FileMode.Create, FileAccess.Write);

            BlockMetadata block = null;

            while (_blockGenerator.TryGetNext(out block))
            {
                compressedFileStream.Seek(block.CompressedPosition, SeekOrigin.Begin);
                var decompressedData = Utils.DecompressBlock(compressedFileStream, block);

                decompressedFileStream.Seek(block.OriginalPosition, SeekOrigin.Begin);
                decompressedFileStream.Write(decompressedData, 0, decompressedData.Length);
            }
        }
    }
}
