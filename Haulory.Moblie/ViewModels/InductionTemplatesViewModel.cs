using Haulory.Application.Interfaces.Services;
using Haulory.Domain.Entities;
using Haulory.Mobile.ViewModels;
using Haulory.Mobile.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;

public class InductionTemplatesViewModel : BaseViewModel
{
    private readonly ISessionService _session;
    private readonly IWorkSiteRepository _sites;
    private readonly IInductionRequirementRepository _reqs;

    public ObservableCollection<WorkSite> WorkSites { get; } = new();
    public ObservableCollection<InductionRequirement> Requirements { get; } = new();

    private WorkSite? _selectedSite;
    public WorkSite? SelectedSite
    {
        get => _selectedSite;
        set
        {
            _selectedSite = value;
            OnPropertyChanged();
            if (value != null) _ = LoadRequirementsAsync(value.Id);
            else Requirements.Clear();
        }
    }

    public ICommand AddSiteCommand { get; }
    public ICommand AddRequirementCommand { get; }
    public ICommand RefreshCommand { get; }

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
            await Shell.Current.GoToAsync(nameof(AddWorkSitePage)));

        AddRequirementCommand = new Command(async () =>
        {
            if (SelectedSite == null)
            {
                await Shell.Current.DisplayAlertAsync("Select a site", "Choose a work site first.", "OK");
                return;
            }

            await Shell.Current.GoToAsync($"{nameof(AddInductionRequirementPage)}?workSiteId={SelectedSite.Id}");
        });
    }

    public async Task LoadAsync()
    {
        await EnsureSessionAsync();
        var ownerId = _session.CurrentAccountId ?? Guid.Empty;
        if (ownerId == Guid.Empty) return;

        WorkSites.Clear();
        Requirements.Clear();

        var sites = await _sites.GetAllByOwnerAsync(ownerId);
        foreach (var s in sites)
            WorkSites.Add(s);

        if (SelectedSite == null && WorkSites.Count > 0)
            SelectedSite = WorkSites[0];
        else if (SelectedSite != null)
            await LoadRequirementsAsync(SelectedSite.Id);
    }

    private async Task LoadRequirementsAsync(Guid workSiteId)
    {
        await EnsureSessionAsync();
        var ownerId = _session.CurrentAccountId ?? Guid.Empty;
        if (ownerId == Guid.Empty) return;

        Requirements.Clear();
        var reqs = await _reqs.GetActiveBySiteAsync(ownerId, workSiteId);
        foreach (var r in reqs)
            Requirements.Add(r);
    }

    private async Task EnsureSessionAsync()
    {
        if (!_session.IsAuthenticated)
            await _session.RestoreAsync();
    }
}
