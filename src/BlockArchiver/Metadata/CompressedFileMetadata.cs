namespace GZipTest.Metadata
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    [JsonConverter(typeof(CompressedMetadataJsonConverter))]
    internal sealed class CompressedFileMetadata
    {
        private readonly ConcurrentBag<BlockMetadata> _blocks = new ConcurrentBag<BlockMetadata>();

        public IEnumerable<BlockMetadata> Blocks
        {
            get { return _blocks; }
        }

        public void Add(BlockMetadata block)
        {
            _blocks.Add(block);
        }
    }
}
