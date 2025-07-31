using DebuggingTool.Services;
using ReactiveUI;
using System.Reactive;

namespace DebuggingTool.ViewModels
{
    public class PLCViewModel : ReactiveObject
    {
        private readonly IVibrationService _vibrationService;
        public ReactiveCommand<Unit, Unit> ChangeTabCommand { get; set; }

        public PLCViewModel(IVibrationService vibrationService)
        {
            _vibrationService = vibrationService;

            ChangeTabCommand = ReactiveCommand.Create(() =>
            {
                _vibrationService?.Vibrate();
            });
        }
    }
}
