namespace GZipTest
{
    internal enum OperationType
    {
        Compress,
        Decompress
    }

    internal class CreateProcessParms
    {
        public OperationType Operation { get; set; }
        public string SourceFile { get; set; }
        public string DestinationFile { get; set; }
    }
}
