namespace GZipTest.TaskManagement
{
    using System;
    using System.Threading;

    internal sealed class TaskRunWrapper
    {
        private readonly ICanceleableTask _task;
        private readonly Action<Exception> _errorHandler;
        private readonly Action _completeHandler;

        public TaskRunWrapper(ICanceleableTask task, Action completeHandler, Action<Exception> errorHandler)
        {
            _task = task;
            _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
            _completeHandler = completeHandler ?? throw new ArgumentNullException(nameof(completeHandler));
        }

        public void Run(object cancellationToken)
        {
            CancellationToken token;

            try
            {
                if (cancellationToken == null)
                    throw new ArgumentNullException(nameof(cancellationToken));

                token = (CancellationToken)cancellationToken;

                _task.Run(token);
                _completeHandler();
            }
            catch (Exception e)
            {
                _errorHandler(e);
            }
        }
    }
}
