using System.Reactive;
using DebuggingTool.Database.Entity;
using DebuggingTool.Services;
using Prism.Regions;
using ReactiveUI;
using RxVoid = ReactiveUI.Primitives.RxVoid;

namespace DebuggingTool.ViewModels
{
    public class HomePageContainerViewModel : ViewModelBase
    {
        private readonly IVibrationService _vibrationService;
        public ReactiveCommand<RxVoid, RxVoid> ChangeTabCommand { get; set; }

        public HomePageContainerViewModel(
            IRegionManager regionManager,
            IVibrationService vibrationService
        )
            : base(regionManager)
        {
            _vibrationService = vibrationService;

            ChangeTabCommand = ReactiveCommand.Create(() =>
            {
                _vibrationService?.Vibrate();
            });
        }
    }
}
