using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Application.Interfaces.Services;
using Haulory.Domain.Entities;
using Haulory.Mobile.ViewModels;
using Haulory.Mobile.Views;
using Microsoft.Maui.Controls;

namespace Haulory.Mobile.ViewModels;

public class InductionTemplatesViewModel : BaseViewModel
{
    #region Dependencies

    private readonly ISessionService _session;
    private readonly IWorkSiteRepository _sites;
    private readonly IInductionRequirementRepository _reqs;

    #endregion

    #region Collections

    public ObservableCollection<WorkSite> WorkSites { get; } = new();
    public ObservableCollection<InductionRequirement> Requirements { get; } = new();

    #endregion

    #region Selected Site

    private WorkSite? _selectedSite;
    public WorkSite? SelectedSite
    {
        get => _selectedSite;
        set
        {
            _selectedSite = value;
            OnPropertyChanged();

            if (value != null)
                _ = LoadRequirementsAsync(value.Id);
            else
                Requirements.Clear();
        }
    }

    #endregion

    #region Commands

    public ICommand AddSiteCommand { get; }
    public ICommand RefreshCommand { get; }

    #endregion

    #region Constructor

    public InductionTemplatesViewModel(
        ISessionService session,
        IWorkSiteRepository sites,
        IInductionRequirementRepository reqs)
    {
        _session = session;
        _sites = sites;
        _reqs = reqs;

        RefreshCommand = new Command(async () => await LoadAsync());

        AddSiteCommand = new Command(async () =>
            await Shell.Current.GoToAsync(nameof(AddWorkSiteTemplatePage)));

    }

    #endregion

    #region Load

    public async Task LoadAsync()
    {
        await EnsureSessionAsync();

        var ownerId = _session.CurrentOwnerId ?? Guid.Empty;
        if (ownerId == Guid.Empty)
            return;

        WorkSites.Clear();
        Requirements.Clear();

        var sites = await _sites.GetAllByOwnerAsync(ownerId);
        foreach (var s in sites)
            WorkSites.Add(s);

        // Keep current behavior: auto-select first site if none is selected
        if (SelectedSite == null && WorkSites.Count > 0)
        {
            SelectedSite = WorkSites[0];
        }
        else if (SelectedSite != null)
        {
            await LoadRequirementsAsync(SelectedSite.Id);
        }
    }

    private async Task LoadRequirementsAsync(Guid workSiteId)
    {
        await EnsureSessionAsync();

        var ownerId = _session.CurrentOwnerId ?? Guid.Empty;
        if (ownerId == Guid.Empty)
            return;

        Requirements.Clear();

        var reqs = await _reqs.GetActiveBySiteAsync(ownerId, workSiteId);
        foreach (var r in reqs)
            Requirements.Add(r);
    }

    #endregion

    #region Session Helper

    private async Task EnsureSessionAsync()
    {
        // Restore session if app restarted
        if (!_session.IsAuthenticated)
            await _session.RestoreAsync();
    }

    #endregion
}
