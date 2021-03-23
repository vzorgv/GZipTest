namespace GZipTest.TaskManagement
{
    using System.Threading;

    internal interface ICanceleableTask
    {
        public void Run(CancellationToken cancellationToken);
    }
}
