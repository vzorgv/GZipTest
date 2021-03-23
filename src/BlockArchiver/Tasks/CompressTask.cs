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
            FileStream inputStream = null;
            FileStream outputStream = null;

            try
            {
                inputStream = new FileStream(_filenameToCompress, FileMode.Open, FileAccess.Read, FileShare.Read);
                outputStream = new FileStream(_compressedFilename, FileMode.Open, FileAccess.Write, FileShare.Write);

                const int OneMB = 1048576; //TODO: remove const
                byte[] dataBuffer = new byte[OneMB];

                long inputPosition = 0;

                while (_inputStreamPositionGenerator.TryGetNext(out inputPosition))
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    ReadData(inputStream, dataBuffer, inputPosition);

                    var compressedDataBuffer = Utils.CompressBuffer(dataBuffer);

                    long outputPosition = 0;
                    GetAndUpdateWritePosition(compressedDataBuffer.Length, out outputPosition);

                    WriteData(outputStream, compressedDataBuffer, outputPosition);
                    WriteMetadataBlock(inputPosition, outputPosition, compressedDataBuffer.Length);
                }
            }
            finally
            {
                if (inputStream != null)
                    inputStream.Dispose();

                if (outputStream != null)
                    outputStream.Dispose();
            }
        }

        private void ReadData(Stream inputStream, byte[] dataBuffer, long position)
        {
            inputStream.Seek(position, SeekOrigin.Begin);
            var readSize = inputStream.Read(dataBuffer, 0, dataBuffer.Length);

            if (readSize < dataBuffer.Length)
            {
                Array.Resize(ref dataBuffer, readSize);
            }
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
