namespace GZipTest.Metadata
{
    using System;

    /// <summary>
    /// The block descriptor
    /// Make it mutable as workaround for json deserialization issue
    /// </summary>
    [Serializable]
    internal sealed class BlockMetadata
    {
        public long OriginalPosition { get; set; }
        public long CompressedPosition { get; set; }
        public int SizeInBytes { get; set; }
    }
}
