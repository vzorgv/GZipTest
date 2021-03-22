namespace GZipTest
{
    internal sealed class StreamPositionGenerator : IBlockGenerator<long>
    {
        private readonly object _syncObject = new object();

        private readonly long _totalElementsCount;
        private long _currentPosition = 0;

        public StreamPositionGenerator(long initialOffset) : this(initialOffset, long.MaxValue)
        {
        }

        public StreamPositionGenerator(long initialOffset, long totalElementsCount)
        {
            _totalElementsCount = totalElementsCount;
            _currentPosition = initialOffset;
        }

        public bool TryGetNext(int requestedSize, out long position)
        {
            lock (_syncObject)
            {
                if (IsNextPositionAllowed())
                {
                    position = _currentPosition;
                    _currentPosition += requestedSize;
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
