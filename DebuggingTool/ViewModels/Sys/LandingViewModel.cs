using DebuggingTool.Region;
using DebuggingTool.Views;
using Prism.Regions;
using System.Threading.Tasks;

namespace DebuggingTool.ViewModels
{
    public class LandingViewModel : ViewModelBase
    {
        public LandingViewModel(IRegionManager regionManager) : base(regionManager)
        {
        }

        public override async void OnIsActiveChanged()
        {
            await Task.Delay(200);
            _regionManager.RequestNavigate(RegionNames.CONTENT_REGION, nameof(HomePageContainerView));
        }
    }
}
