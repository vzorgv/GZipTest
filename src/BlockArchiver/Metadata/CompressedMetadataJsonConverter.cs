namespace GZipTest.Metadata
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    internal sealed class CompressedMetadataJsonConverter : JsonConverter<CompressedFileMetadata>
    {
        public override CompressedFileMetadata Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var metadata = new CompressedFileMetadata();

            var list = JsonSerializer.Deserialize<List<BlockMetadata>>(ref reader);

            foreach (var block in list)
            {
                metadata.Add(block);
            }

            return metadata;
        }

        public override void Write(Utf8JsonWriter writer, CompressedFileMetadata value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            foreach (var block in value.Blocks)
            {
                JsonSerializer.Serialize(writer, block);
            }

            writer.WriteEndArray();
        }
    }
}
