using avalonia_new.Events;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using System;
using System.Windows.Input;

namespace avalonia_new.ViewModels
{
    public class HomePageContainerViewModel : ViewModelBase
    {
        bool _ShowPopup = false;
        public bool ShowPopup
        {
            get => _ShowPopup;
            set => SetProperty(ref _ShowPopup, value);
        }
        public ICommand ChangeTabCommand { get; }
        public HomePageContainerViewModel(IRegionManager regionManager, IEventAggregator eventAggregator) : base(regionManager)
        {
            ChangeTabCommand = new DelegateCommand<object>(ChangeTab);
            eventAggregator.GetEvent<PopUpEvent>().Subscribe(OnPopUpDisplayed);
        }

        private void OnPopUpDisplayed(PopUpEventData data)
        {
            ShowPopup = data.IsDisplayed;
        }

        private void ChangeTab(object tabIndex)
        {

        }
    }
}
