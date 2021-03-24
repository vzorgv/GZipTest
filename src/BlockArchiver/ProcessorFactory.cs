namespace GZipTest
{
    using GZipTest.Processor;

    internal class ProcessorFactory
    {
        private readonly CreateProcessParms _parms;
        private readonly int _blockSize;

        public ProcessorFactory(CreateProcessParms parms, int blockSize)
        {
            _parms = parms;
            _blockSize = blockSize;
        }

        public IArchiverProcessor Create()
        {
            IArchiverProcessor processor = null;

            switch (_parms.Operation)
            {
                case OperationType.Compress:
                    processor = new Compressor(_parms.SourceFile, _parms.DestinationFile, _blockSize);
                    break;

                case OperationType.Decompress:
                    processor = new Decompressor(_parms.SourceFile, _parms.DestinationFile);
                    break;
            }

            return processor;
        }
    }
}
