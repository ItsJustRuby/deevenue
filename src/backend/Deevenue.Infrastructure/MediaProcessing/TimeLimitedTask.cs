namespace Deevenue.Infrastructure.MediaProcessing;

internal static class TimeLimitedTask
{
    public static async Task<TimeLimitedTaskResult<T>> Run<T>(
        TimeSpan timeLimit,
        Func<CancellationToken, Task<T>> createMainTask)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var timeoutTask = Task.Delay(timeLimit, cancellationToken);

        var mainTask = createMainTask(cancellationToken);

        var firstTaskToComplete = await Task.WhenAny(mainTask, timeoutTask);

        if (firstTaskToComplete == timeoutTask)
        {
            try
            {
                cancellationTokenSource.Cancel();
            }
            catch (Exception)
            {
                // There is a bug in FFMpegArgumentProcessor's OnCancelEvent
                // that just straight up calls instance.Kill() without checking
                // if the process is even still there. That throws on timeout.
            }
            return new TimeLimitedTaskResult<T>(true, default);
        }

        return new TimeLimitedTaskResult<T>(false, await mainTask);
    }
}

internal record TimeLimitedTaskResult<T>(bool ExceededTimeLimit, T? Result);

