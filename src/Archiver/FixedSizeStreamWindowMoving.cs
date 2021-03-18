namespace GZipTest
{
    using System.IO;

    /// <summary>
    /// Implements moving fixed size window
    /// </summary>
    internal class FixedSizeStreamWindowMoving : IStreamWindowMovingStrategy
    {
        private readonly int _windowSize;
        private readonly long _totalElementsCount;
        private long _currentOffset;

        public FixedSizeStreamWindowMoving(int windowSize, Stream stream)
        {
            _windowSize = windowSize;
            _totalElementsCount = stream.Length;
            _currentOffset = stream.Position;
        }

        public bool CanBeMoved()
        {
            return _currentOffset < _totalElementsCount;
        }

        public StreamWindow Move()
        {
            var ret = new StreamWindow(_currentOffset, _windowSize);
            _currentOffset += _windowSize;

            return ret;
        }
    }
}
