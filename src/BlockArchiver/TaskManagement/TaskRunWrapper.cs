namespace GZipTest.TaskManagement
{
    using System;
    using System.Threading;

    internal sealed class TaskRunWrapper
    {
        private readonly ICanceleableTask _task;

        public TaskRunWrapper(ICanceleableTask task)
        {
            _task = task;
        }

        public void Run(object cancellationToken)
        {
            CancellationToken token;

            if (cancellationToken == null)
                throw new ArgumentNullException(nameof(cancellationToken));

            token = (CancellationToken)cancellationToken;

            _task.Run(token);
        }
    }
}
