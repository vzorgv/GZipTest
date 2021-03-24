namespace GZipTest.Tasks
{
    using GZipTest.BlockGenerators;
    using GZipTest.Metadata;
    using GZipTest.TaskManagement;
    using System;
    using System.IO;
    using System.Threading;

    internal sealed class CompressTask : ICanceleableTask
    {
        private readonly FixedSizeBlockGenerator _inputStreamPositionGenerator;
        private readonly CompressedFileMetadata _fileMetadata;
        private readonly string _filenameToCompress;
        private readonly string _compressedFilename;

        private readonly object _syncObj = new object();

        private long _writePosition;

        public CompressTask(string filenameToCompress, string compressedFilename, FixedSizeBlockGenerator inputStreamPositionGenerator, CompressedFileMetadata fileMetadata, long initialWritePosition)
        {
            _inputStreamPositionGenerator = inputStreamPositionGenerator;
            _filenameToCompress = filenameToCompress;
            _compressedFilename = compressedFilename;
            _fileMetadata = fileMetadata;
            _writePosition = initialWritePosition;
        }

        public void Run(CancellationToken cancellationToken)
        {
            Compress(cancellationToken);
        }

        private void Compress(CancellationToken cancellationToken)
        {
            using var inputStream = new FileStream(_filenameToCompress, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var outputStream = new FileStream(_compressedFilename, FileMode.Open, FileAccess.Write, FileShare.Write);

            byte[] dataBuffer = new byte[_inputStreamPositionGenerator.BlockSize];

            long inputPosition = 0;
  
            while (_inputStreamPositionGenerator.TryGetNext(out inputPosition))
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                dataBuffer = ReadData(inputStream, dataBuffer, inputPosition);

                var compressedDataBuffer = Utils.CompressBuffer(dataBuffer);

                GetAndUpdateWritePosition(compressedDataBuffer.Length, out long outputPosition);

                WriteData(outputStream, compressedDataBuffer, outputPosition);
                WriteMetadataBlock(inputPosition, outputPosition, compressedDataBuffer.Length);
            }
        }

        private byte[] ReadData(Stream inputStream, byte[] dataBuffer, long position)
        {
            byte[] data = dataBuffer;
            inputStream.Seek(position, SeekOrigin.Begin);
            var readSize = inputStream.Read(dataBuffer, 0, dataBuffer.Length);

            if (readSize < dataBuffer.Length)
            {
                Array.Resize(ref data, readSize);
            }

            return data; 
        }

        private void WriteData(Stream outputStream, byte[] dataBuffer, long position)
        {
            outputStream.Seek(position, SeekOrigin.Begin);
            outputStream.Write(dataBuffer);
        }

        private void WriteMetadataBlock(long originalPosition, long compressedPosition, int size)
        {
            var blockMetadata = new BlockMetadata
            {
                OriginalPosition = originalPosition,
                CompressedPosition = compressedPosition,
                SizeInBytes = size
            };

            _fileMetadata.Add(blockMetadata);
        }

        private void GetAndUpdateWritePosition(int addValue, out long position)
        {
            lock (_syncObj)
            {
                position = _writePosition;
                _writePosition += addValue;
            }
        }
    }
}
