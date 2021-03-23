namespace GZipTest.BlockGenerators
{
    using GZipTest.Metadata;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.Json;

    internal sealed class BlockMetadataGenerator : IBlockGenerator<BlockMetadata>
    {
        private readonly object _syncObject = new object();

        IEnumerator<BlockMetadata> _enumerator;

        public BlockMetadataGenerator(Stream metatadaStream)
        {
            _enumerator = DecompressMetadata(metatadaStream).Blocks.GetEnumerator();
        }

        public bool TryGetNext(out BlockMetadata current)
        {
            lock (_syncObject)
            {
                var ret = _enumerator.MoveNext();
                current = ret ? _enumerator.Current : default(BlockMetadata);

                return ret;
            }
        }

        private CompressedFileMetadata DecompressMetadata(Stream compressedMetadataStream)
        {
            using var streamReader = new StreamReader(compressedMetadataStream);

            var jsonString = streamReader.ReadToEnd();
            
            return JsonSerializer.Deserialize<CompressedFileMetadata>(jsonString);
        }
    }
}
