namespace GZipTest
{
    using System;
    using System.Diagnostics;

    class Program
    {
        static void Main(string[] args)
        {
            const int OneMB = 1048576;

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            var archiver = new BlockArchiver();
            archiver.Compress(@"C:\temp\_1.txt", @"C:\Temp\_2.gzt", OneMB);
            archiver.Decompress(@"C:\Temp\_2.gzt", @"C:\temp\_decompressed.txt");

            stopWatch.Stop();

            TimeSpan ts = stopWatch.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("Compress duration " + elapsedTime);
        }
    }
}
