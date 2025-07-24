namespace Station.Infrastructure.Models;

public class TaskManager
{
    private volatile bool _canceled;

    public bool Canceled => _canceled;

    public void Cancel()
    {
        _canceled = true;
    }

    public void Reset()
    {
        _canceled = false;
    }
}