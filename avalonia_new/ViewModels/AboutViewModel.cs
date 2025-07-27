using Prism.Commands;
using Prism.Regions;
using System.Windows.Input;

namespace avalonia_new.ViewModels
{
    public class AboutViewModel : ViewModelBase
    {
        public ICommand GoToCommand { get; }
        public AboutViewModel(IRegionManager regionManager) : base(regionManager)
        {
            GoToCommand = new DelegateCommand(GotoGithub);
        }

        private void GotoGithub()
        {
           
        }
    }
}
