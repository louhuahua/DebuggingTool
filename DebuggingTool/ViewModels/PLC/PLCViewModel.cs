using DebuggingTool.Services;
using ReactiveUI;
using RxVoid = ReactiveUI.Primitives.RxVoid;

namespace DebuggingTool.ViewModels
{
    public class PLCViewModel : ReactiveObject
    {
        private readonly IVibrationService _vibrationService;
        public ReactiveCommand<RxVoid, RxVoid> ChangeTabCommand { get; set; }

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
