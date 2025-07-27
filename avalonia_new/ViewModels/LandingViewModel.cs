using avalonia_new.Region;
using avalonia_new.Views;
using Prism.Regions;
using System.Threading.Tasks;

namespace avalonia_new.ViewModels
{
    public class LandingViewModel : ViewModelBase
    {
        public LandingViewModel(IRegionManager regionManager) : base(regionManager)
        {
        }

        public override async void OnIsActiveChanged()
        {
            await Task.Delay(200);
            _regionManager.RequestNavigate(RegionNames.CONTENT_REGION, nameof(LoginView));
        }
    }
}
