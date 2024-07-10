namespace LabExtended.Utilities.Async
{
    public static class AsyncRunner
    {
        public static AsyncOperation RunAsync(Action action)
        {
            if (action is null)
                throw new ArgumentNullException(nameof(action));

            var operation = new AsyncOperation();

            var task = Task.Run(() =>
            {
                try
                {
                    action();
                    operation.Report(null);
                }
                catch (Exception ex)
                {
                    operation.Report(ex);
                }
            });

            return operation;
        }

        public static AsyncOperation<T> RunAsync<T>(Func<T> func)
        {
            var operation = new AsyncOperation<T>();

            var task = Task.Run(() =>
            {
                try
                {
                    operation.Report(null, func());
                }
                catch (Exception ex)
                {
                    operation.Report(ex, default);
                }
            });

            return operation;
        }

        public static AsyncOperation<T> RunAsync<T>(Task<T> func)
        {
            var operation = new AsyncOperation<T>();

            var task = Task.Run(async () =>
            {
                try
                {
                    await func;

                    operation.Report(func.Exception, func.Result);
                }
                catch (Exception ex)
                {
                    operation.Report(ex, default);
                }
            });

            return operation;
        }

        public static AsyncOperation RunThreadAsync(Action action)
        {
            var operation = new AsyncOperation();

            var thread = new Thread(() =>
            {
                try
                {
                    action();
                    operation.Report(null);
                }
                catch (Exception ex)
                {
                    operation.Report(ex);
                }
            });

            thread.Start();
            return operation;
        }

        public static AsyncOperation<T> RunThreadAsync<T>(Task<T> func)
        {
            var operation = new AsyncOperation<T>();

            var thread = new Thread(async () =>
            {
                try
                {
                    await func;

                    operation.Report(func.Exception, func.Result);
                }
                catch (Exception ex)
                {
                    operation.Report(ex, default);
                }
            });

            thread.Start();
            return operation;
        }

        public static AsyncOperation<T> RunThreadAsync<T>(Func<T> func)
        {
            var operation = new AsyncOperation<T>();

            var thread = new Thread(() =>
            {
                try
                {
                    operation.Report(null, func());
                }
                catch (Exception ex)
                {
                    operation.Report(ex, default);
                }
            });

            thread.Start();
            return operation;
        }
    }
}