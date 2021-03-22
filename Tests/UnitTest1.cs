using GZipTest;
using NUnit.Framework;

namespace Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            var archiver = new BlockArchiver();
            archiver.Compress(@"C:\temp\_1.txt", @"C:\Temp\xxx.gzt");
        }
    }
}