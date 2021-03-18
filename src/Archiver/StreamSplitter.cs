namespace GZipTest
{
    internal class StreamSplitter : IAtomicEnumerator<StreamWindow>
    {
        private readonly IStreamWindowMovingStrategy _movingStrategy;
        private readonly object _syncObject = new object();

        public StreamSplitter(IStreamWindowMovingStrategy movingStrategy)
        {
            _movingStrategy = movingStrategy;
        }

        public bool TryMoveNext(out StreamWindow current)
        {
            lock (_syncObject)
            {
                bool hasNext = _movingStrategy.CanBeMoved();
                current = hasNext ? _movingStrategy.Move() : default;

                return hasNext;
            }
        }
    }
}
