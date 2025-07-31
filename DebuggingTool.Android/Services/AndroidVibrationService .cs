using Android.Content;
using Android.OS;
using DebuggingTool.Services;

namespace DebuggingTool.Android.Services;

public class AndroidVibrationService : IVibrationService
{
    public void Vibrate(int milliseconds)
    {
        var context = global::Android.App.Application.Context;
        using (var vibrator = (Vibrator)context.GetSystemService(Context.VibratorService))
        {
            if (!vibrator.HasVibrator) return;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                vibrator.Vibrate(VibrationEffect.CreateOneShot(milliseconds, VibrationEffect.DefaultAmplitude));
            }
            else
            {
                vibrator.Vibrate(milliseconds);
            }
        }
    }
}
