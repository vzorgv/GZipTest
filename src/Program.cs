using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Tests")]
namespace GZipTest
{
    using System;
    using System.Diagnostics;

    class Program
    {
        private static void PrintHelp()
        {
            Console.WriteLine("Please use:");
            Console.WriteLine("GZipTest compress <file_to_compress> <compressed_file>");
            Console.WriteLine("or");
            Console.WriteLine("GZipTest decompress <file_to_decompress> <decompressed_file>");
        }

        private static CreateProcessParms ParseCommandLine(string[] args)
        {
            var parms = new CreateProcessParms();

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

        private static void Run(CreateProcessParms parms)
        {
            const int OneMb = 1048576;

            var processor = new ProcessorFactory(parms, OneMb)
                    .Create();

            processor.StartProcess();
        }

        static int Main(string[] args)
        {
            int ret = 0;
            CreateProcessParms parms;

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
