using Prism.Mvvm;
using Prism.Regions;
using avalonia_new.Views;
using avalonia_new.Region;
using System;
using Prism.Commands;
using Prism;

namespace avalonia_new.ViewModels;

public class ViewModelBase : BindableBase, INavigationAware, IActiveAware
{
    private readonly string _baseNavPage = nameof(LoginView);
    public readonly IRegionManager _regionManager;
    private IRegionNavigationJournal? _journal;
    public IRegionNavigationJournal Journal
    {
        get
        {
            if (_journal == null)
            {
                _journal = _regionManager.Regions[RegionNames.CONTENT_REGION].NavigationService.Journal;
            }
            return _journal;
        }
    }

    private bool _isActive;
    public event EventHandler IsActiveChanged;

    public bool IsActive
    {
        get { return _isActive; }
        set { SetProperty(ref _isActive, value, () => OnIsActiveChanged()); }
    }


    public ViewModelBase(IRegionManager regionManager)
    {
        _regionManager = regionManager ?? throw new ArgumentNullException(nameof(regionManager));
        if (_regionManager.Regions.ContainsRegionWithName(RegionNames.CONTENT_REGION))
        {
            _journal = _regionManager.Regions[RegionNames.CONTENT_REGION].NavigationService.Journal;
        }

    }
    public virtual void OnIsActiveChanged()
    {

    }
    public IRegionManager Navigation => _regionManager;

    public virtual DelegateCommand GoBackCommand => new DelegateCommand(() =>
    {
        // Go back to the previous calling page, otherwise, Dashboard.
        if (_journal != null && _journal.CanGoBack)
            _journal.GoBack();
        else
            _regionManager.RequestNavigate(RegionNames.CONTENT_REGION, _baseNavPage);
    });


    public virtual bool IsNavigationTarget(NavigationContext navigationContext)
    {
        // Auto-allow navigation
        return OnNavigatingTo(navigationContext);
    }

    /// <summary>Called when the implementer is being navigated away from.</summary>
    /// <param name="navigationContext">The navigation context.</param>
    public virtual void OnNavigatedFrom(NavigationContext navigationContext)
    {
    }

    /// <summary>Called when the implementer has been navigated to.</summary>
    /// <param name="navigationContext">The navigation context.</param>
    public virtual void OnNavigatedTo(NavigationContext navigationContext)
    {
    }

    /// <summary>Navigation validation checker.</summary>
    /// <remarks>Override for Prism 7.2's IsNavigationTarget.</remarks>
    /// <param name="navigationContext">The navigation context.</param>
    /// <returns><see langword="true"/> if this instance accepts the navigation request; otherwise, <see langword="false"/>.</returns>
    public virtual bool OnNavigatingTo(NavigationContext navigationContext)
    {
        return true;
    }
}
