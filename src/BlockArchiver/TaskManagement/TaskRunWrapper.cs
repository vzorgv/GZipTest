namespace GZipTest.TaskManagement
{
    using System;
    using System.Threading;

    internal sealed class TaskRunWrapper
    {
        private readonly ICanceleableTask _task;
        private readonly Action<Exception> _errorHandler;

        public TaskRunWrapper(ICanceleableTask task, Action<Exception> errorHandler)
        {
            if (errorHandler == null)
                throw new ArgumentNullException(nameof(errorHandler));

            _task = task;
            _errorHandler = errorHandler;
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
            }
            catch (Exception e)
            {
                _errorHandler(e);
            }
        }
    }
}
