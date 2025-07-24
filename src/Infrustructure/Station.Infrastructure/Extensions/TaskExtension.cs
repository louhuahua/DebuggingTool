namespace Station.Infrastructure.Extensions;

public static class TaskExtension
{
    public static async void Await(this Task task, Action onComplete, Action<Exception> onException = null)
    {
        try
        {
            await task;
            onComplete.Invoke();
        }
        catch (Exception e)
        {
            onException?.Invoke(e);
        }
    }
    
    public static async void Await<T>(this Task<T> task, Func<T, T> onComplete, Action<Exception> onException = null)
    {
        try
        {
            var resultList = await task;
            onComplete.Invoke(resultList);
        }
        catch (Exception e)
        {
            onException?.Invoke(e);
        }
    }
}