namespace GZipTest.BlockGenerators
{
    internal sealed class FixedSizeBlockGenerator : IBlockGenerator<long>
    {
        private readonly object _syncObject = new object();

        private readonly long _totalElementsCount;
        private long _currentPosition = 0;

        public FixedSizeBlockGenerator(int blockSize, long totalElementsCount)
        {
            _totalElementsCount = totalElementsCount;
            BlockSize = blockSize;
        }

        public int BlockSize { get; }

        public bool TryGetNext(out long position)
        {
            lock (_syncObject)
            {
                if (IsNextPositionAllowed())
                {
                    position = _currentPosition;
                    _currentPosition += BlockSize;
                    return true;
                }
                else
                {
                    position = 0;
                    return false;
                }
            }
        }

        bool IsNextPositionAllowed()
        {
            return _currentPosition < _totalElementsCount;
        }
    }
}
