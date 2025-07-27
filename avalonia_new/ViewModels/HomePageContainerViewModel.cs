using System;
using System.Windows.Input;
using avalonia_new.Events;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using ReactiveUI.Fody.Helpers;

namespace avalonia_new.ViewModels
{
    public class HomePageContainerViewModel : ViewModelBase
    {
        [Reactive]
        public bool ShowPopup { get; set; }
        public ICommand ChangeTabCommand { get; }

        public HomePageContainerViewModel(
            IRegionManager regionManager,
            IEventAggregator eventAggregator
        )
            : base(regionManager)
        {
            ChangeTabCommand = new DelegateCommand<object>(ChangeTab);
            eventAggregator.GetEvent<PopUpEvent>().Subscribe(OnPopUpDisplayed);
        }

        private void OnPopUpDisplayed(PopUpEventData data)
        {
            ShowPopup = data.IsDisplayed;
        }

        private void ChangeTab(object tabIndex) { }
    }
}
