using DebuggingTool.Model;
using DebuggingTool.Services;
using ReactiveUI;

namespace DebuggingTool.Desktop;

public class DesktopVibrationService : IVibrationService
{
    public void Vibrate(int milliseconds)
    {
        //MessageBus.Current.SendMessage(new SnackBarMessage($"Vibrate from desktop", 3));
    }
}
