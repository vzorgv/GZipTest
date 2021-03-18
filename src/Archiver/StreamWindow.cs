namespace GZipTest
{
    /// <summary>
    /// The window of a <see cref="System.IO.Stream"/> object
    /// </summary>
    internal sealed class StreamWindow
    {
        /// <summary>
        /// Position of the Window
        /// </summary>
        public long Position { get; }

        /// <summary>
        /// Size of the Window
        /// </summary>
        public int Size { get; }

        public StreamWindow(long position, int size)
        {
            Position = position;
            Size = size;
        }
    }
}
