namespace GZipTest.Tasks
{
    using GZipTest.BlockGenerators;
    using GZipTest.Metadata;
    using GZipTest.TaskManagement;
    using System;
    using System.IO;

    internal sealed class CompressTask : IRunnable
    {
        private readonly StreamPositionGenerator _inputStreamPositionGenerator;
        private readonly StreamPositionGenerator _outputStreamPositionGenerator;
        private readonly CompressedFileMetadata _fileMetadata;
        private readonly string _filenameToCompress;
        private readonly string _compressedFilename;

        public CompressTask(string filenameToCompress, string compressedFilename, StreamPositionGenerator inputStreamPositionGenerator, StreamPositionGenerator outputStreamPositionGenerator, CompressedFileMetadata fileMetadata)
        {
            _inputStreamPositionGenerator = inputStreamPositionGenerator;
            _outputStreamPositionGenerator = outputStreamPositionGenerator;
            _filenameToCompress = filenameToCompress;
            _compressedFilename = compressedFilename;
            _fileMetadata = fileMetadata;
        }

        public void Run()
        {
            //TODO: implement gracefully shutdown
            Compress();
        }

        private void Compress()
        {
            FileStream inputStream = null;
            FileStream outputStream = null;

            try
            {
                inputStream = new FileStream(_filenameToCompress, FileMode.Open, FileAccess.Read, FileShare.Read);
                outputStream = new FileStream(_compressedFilename, FileMode.Open, FileAccess.Write, FileShare.Write);

                byte[] dataBuffer = null;

                long inputPosition = 0;
                long outputPosition = 0;

                const int OneMB = 1048576; //TODO: remove const

                while (_inputStreamPositionGenerator.TryGetNext(OneMB, out inputPosition))
                {
                    // TODO: extract read&write window logic to abstraction
                    if (dataBuffer == null)
                    {
                        dataBuffer = new byte[OneMB];
                    }

                    inputStream.Seek(inputPosition, SeekOrigin.Begin);
                    var readSize = inputStream.Read(dataBuffer, 0, dataBuffer.Length);

                    if (readSize < OneMB)
                    {
                        Array.Resize(ref dataBuffer, readSize);
                    }

                    var compressedDataBuffer = Utils.CompressBuffer(dataBuffer);

                    _outputStreamPositionGenerator.TryGetNext(compressedDataBuffer.Length, out outputPosition);

                    outputStream.Seek(outputPosition, SeekOrigin.Begin);
                    outputStream.Write(compressedDataBuffer);

                    var blockMetadata = new BlockMetadata
                    {
                        OriginalPosition = inputPosition,
                        CompressedPosition = outputPosition,
                        SizeInBytes = compressedDataBuffer.Length
                    };

                    _fileMetadata.Add(blockMetadata);
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
    }
}
