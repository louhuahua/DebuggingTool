using System.Windows.Input;
using DebuggingTool.Region;
using DebuggingTool.Views;
using Prism.Commands;
using Prism.Regions;
using ReactiveUI.SourceGenerators;

namespace DebuggingTool.ViewModels
{
    public partial class LoginViewModel : ViewModelBase
    {
        [Reactive]
        public partial string Username { get; set; }

        [Reactive]
        public partial string Password { get; set; }
        public ICommand LoginCommand { get; }

        public LoginViewModel(IRegionManager regionManager)
            : base(regionManager)
        {
            LoginCommand = new DelegateCommand(LoginUser);
        }

        private void LoginUser()
        {
            _regionManager.RequestNavigate(
                RegionNames.CONTENT_REGION,
                nameof(HomePageContainerView)
            );
        }
    }
}
