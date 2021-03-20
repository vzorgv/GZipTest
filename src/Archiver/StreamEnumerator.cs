namespace GZipTest
{
    /// <summary>
    /// Implements enumerator on the stream
    /// </summary>
    internal class StreamEnumerator : IAtomicEnumerator<StreamWindow>
    {
        private readonly IStreamWindowMovingStrategy _movingStrategy;
        private readonly object _syncObject = new object();

        public StreamEnumerator(IStreamWindowMovingStrategy movingStrategy)
        {
            _movingStrategy = movingStrategy;
        }

        public bool TryMoveNext(out StreamWindow current)
        {
            //TODO: try to use non blocking
            lock (_syncObject)
            {
                bool hasNext = _movingStrategy.CanBeMoved();
                current = hasNext ? _movingStrategy.Move() : default;

                return hasNext;
            }
        }
    }
}
