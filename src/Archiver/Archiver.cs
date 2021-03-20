namespace GZipTest
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Threading;

    public class Archiver
    {
        public void Compress(string fileNameToCompress, string compressedFileName)
        {
            List<Thread> threads = new List<Thread>();


            using var sourceStream = new FileStream(fileNameToCompress, FileMode.Open, FileAccess.Read, FileShare.Read);
            //TODO: remove const
            var windowMovingStrategy = new FixedSizeStreamWindowMoving(1048576, sourceStream);
            var windowEnumerator = sourceStream.GetWindowEnumerator(windowMovingStrategy);


            //TODO: remove const
            for (int i = 0; i < CalcThreadCount(100); i++)
            {
                var thread = new Thread(CompressWorker);
                threads.Add(thread);
                thread.Start(windowEnumerator);
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }
        }

        public void Decompress(string compressedFileName, string decompressedFileName)
        {
        }


        private static void CompressWorker(object windowEnmerator)
        {
            if (windowEnmerator == null)
                throw new ArgumentNullException(nameof(windowEnmerator));

            var windowEnumerator = windowEnmerator as IAtomicEnumerator<StreamWindow>;

            if (windowEnumerator == null)
                throw new Exception("Invalid type of argument");

            //TODO: remove const
            using FileStream stream = new FileStream(@"C:\Temp\_1.txt", FileMode.Open, FileAccess.Read, FileShare.Read);
            StreamWindow window;

            while (windowEnumerator.TryMoveNext(out window))
            {
                // TODO: extract read&write window logic to abstraction
                var data = new byte[window.Size];
                stream.Seek(window.Position, SeekOrigin.Begin);
                var readSize = stream.Read(data, 0, window.Size);

                if (readSize < window.Size)
                    Array.Resize(ref data, readSize);

                using var memStream = new MemoryStream(data);
                using var zippedMemory = new MemoryStream();
                using var zipStream = new GZipStream(zippedMemory, CompressionMode.Compress);

                memStream.CopyTo(zippedMemory);
            }

        }

        private int CalcThreadCount(int countOfCunks)
        {
            if (countOfCunks == 0)
            {
                return 0;
            }
            else
            {
                return Math.Min(countOfCunks, Environment.ProcessorCount);
            }
        }
    }
}
