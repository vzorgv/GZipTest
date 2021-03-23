namespace GZipTest.Tasks
{
    using GZipTest.BlockGenerators;
    using GZipTest.Metadata;
    using GZipTest.TaskManagement;
    using System.IO;
    using System.Threading;

    internal sealed class DecompressTask : ICanceleableTask
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

        public void Run(CancellationToken cancellationToken)
        {
            Decompress(cancellationToken);
        }

        private void Decompress(CancellationToken cancellationToken)
        {
            using var compressedFileStream = new FileStream(_compressedFilename, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var decompressedFileStream = new FileStream(_decompressedFileName, FileMode.Create, FileAccess.Write, FileShare.Write);

            BlockMetadata block = null;

            while (_blockGenerator.TryGetNext(out block))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                compressedFileStream.Seek(block.CompressedPosition, SeekOrigin.Begin);
                var decompressedData = Utils.DecompressBlock(compressedFileStream, block);

                decompressedFileStream.Seek(block.OriginalPosition, SeekOrigin.Begin);
                decompressedFileStream.Write(decompressedData, 0, decompressedData.Length);
            }
        }
    }
}
