using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Tests")]
namespace GZipTest
{
    using GZipTest.BlockArchiver;
    using System;
    using System.Diagnostics;

    class Program
    {
        internal enum OperationType
        {
            Compress,
            Decompress
        }

        internal class Parameters
        {
            public OperationType Operation { get; set; }
            public string SourceFile { get; set; }
            public string DestinationFile { get; set; }
        }

        private static void PrintHelp()
        {
            Console.WriteLine("Please use:");
            Console.WriteLine("GZipTest compress <file_to_compress> <compressed_file>");
            Console.WriteLine("or");
            Console.WriteLine("GZipTest decompress <file_to_decompress> <decompressed_file>");
        }

        private static Parameters ParseCommandLine(string[] args)
        {
            var parms = new Parameters();

            if (args.Length != 3)
            {
                throw new Exception("Error in command line");
            }

            switch (args[0])
            {
                case "compress":
                    parms.Operation = OperationType.Compress;
                    break;
                case "decompress":
                    parms.Operation = OperationType.Decompress;
                    break;

                default:
                    throw new Exception("Incorrect operation type");
            }

            parms.SourceFile = args[1];
            parms.DestinationFile = args[2];

            return parms;
        }

        private static void Run(Parameters parms)
        {
            const int OneMB = 1048576;

            switch (parms.Operation)
            {
                case OperationType.Compress:
                    var compressor = new Compressor(parms.SourceFile, parms.DestinationFile, OneMB);
                    compressor.Run();
                    break;

                case OperationType.Decompress:
                    var decompressor = new Decompressor(parms.SourceFile, parms.DestinationFile);
                    decompressor.Run();
                    break;
            }
        }

        static int Main(string[] args)
        {
            int ret = 0;
            Parameters parms;

            try
            {
                parms = ParseCommandLine(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                PrintHelp();
                return 1;
            }

            try
            {
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                Console.WriteLine("Processing...");

                Run(parms);

                stopWatch.Stop();

                TimeSpan ts = stopWatch.Elapsed;

                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                    ts.Hours, ts.Minutes, ts.Seconds,
                    ts.Milliseconds / 10);
                Console.WriteLine("Process duration " + elapsedTime);
            }
            catch
            {
                Console.WriteLine("Process terminated");
                ret = 1;
            }
            //TODO: handle SIGINT, SIGTERM
            return ret;
        }
    }
}
