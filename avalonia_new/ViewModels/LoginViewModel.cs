using System.Windows.Input;
using avalonia_new.Region;
using avalonia_new.Views;
using Prism.Commands;
using Prism.Regions;
using ReactiveUI.Fody.Helpers;

namespace avalonia_new.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        [Reactive]
        public string Username { get; set; }

        [Reactive]
        public string Password { get; set; }
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
