using GZipTest;
using GZipTest.Archiver;
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
        }
    }
}