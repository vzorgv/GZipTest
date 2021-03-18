namespace GZipTest
{
    using System.IO;

    internal static class StreamExtension
    {
        public static StreamEnumerator GetWindowEnumerator(this Stream _stream, IStreamWindowMovingStrategy movingStrategy)
        {
            return new StreamEnumerator(movingStrategy);
        }
    }
}
