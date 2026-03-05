using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Haulory.Application.Features.Incductions;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Application.Interfaces.Services;
using Haulory.Domain.Entities;
using Haulory.Mobile.Views;
using Microsoft.Maui.Controls;

namespace Haulory.Mobile.ViewModels;

public class ManageInductionsViewModel : BaseViewModel
{
    #region Dependencies

    private readonly ISessionService _session;
    private readonly IDriverRepository _drivers;
    private readonly IDriverInductionRepository _driverInductions;
    private readonly IComplianceEnsurer _ensurer;
    private readonly IWorkSiteRepository _sites;

    #endregion

    #region State

    private bool _isBusy;
    private Driver? _selectedDriver;

    #endregion

    #region Bindable Properties

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            _isBusy = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<Driver> Drivers { get; } = new();
    public ObservableCollection<DriverInductionListItemDto> Inductions { get; } = new();

    public Driver? SelectedDriver
    {
        get => _selectedDriver;
        set
        {
            _selectedDriver = value;
            OnPropertyChanged();

            // Fire and forget load for inductions list
            _ = LoadInductionsAsync();
        }
    }

    #endregion

    #region Commands

    public ICommand RefreshCommand { get; }
    public ICommand GoToTemplatesCommand { get; }
    public ICommand AddInductionCommand { get; }
    public ICommand EditCommand { get; }

    #endregion

    #region Constructor

    public ManageInductionsViewModel(
        ISessionService session,
        IDriverRepository drivers,
        IDriverInductionRepository driverInductions,
        IWorkSiteRepository sites,
        IComplianceEnsurer ensurer)
    {
        _session = session;
        _drivers = drivers;
        _driverInductions = driverInductions;
        _sites = sites;
        _ensurer = ensurer;

        RefreshCommand = new Command(async () => await LoadAsync());

        GoToTemplatesCommand = new Command(async () =>
            await Shell.Current.GoToAsync(nameof(InductionTemplatesPage)));

        AddInductionCommand = new Command(async () => await AddInductionAsync());

        EditCommand = new Command<DriverInductionListItemDto>(async item =>
        {
            if (item == null || SelectedDriver == null)
                return;

            await EditAsync(item);
        });
    }

    #endregion

    #region Load Drivers

    public async Task LoadAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            await EnsureSessionAsync();

            var ownerId = _session.CurrentOwnerId ?? Guid.Empty;
            if (ownerId == Guid.Empty)
                return;

            Drivers.Clear();
            Inductions.Clear();

            var drivers = await _drivers.GetAllByOwnerUserIdAsync(ownerId);
            foreach (var d in drivers.OrderBy(d => d.DisplayName))
                Drivers.Add(d);

            if (SelectedDriver == null && Drivers.Count > 0)
                SelectedDriver = Drivers[0];
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Load failed", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    #endregion

    #region Load Inductions

    private async Task LoadInductionsAsync()
    {
        try
        {
            Inductions.Clear();

            if (SelectedDriver == null)
                return;

            await EnsureSessionAsync();

            var ownerId = _session.CurrentOwnerId ?? Guid.Empty;
            if (ownerId == Guid.Empty)
                return;

            var list = await _driverInductions.GetListItemsByDriverAsync(ownerId, SelectedDriver.Id);
            foreach (var x in list)
                Inductions.Add(x);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Load failed", ex.Message, "OK");
        }
    }

    #endregion

    #region Edit Induction

    private async Task EditAsync(DriverInductionListItemDto item)
    {
        if (SelectedDriver == null)
            return;

        // 1) Status
        var statusText = await Shell.Current.DisplayActionSheet(
            "Set Status",
            "Cancel",
            null,
            ComplianceStatus.NotStarted.ToString(),
            ComplianceStatus.InProgress.ToString(),
            ComplianceStatus.Completed.ToString(),
            ComplianceStatus.Expired.ToString());

        if (statusText == "Cancel" || !Enum.TryParse(statusText, out ComplianceStatus newStatus))
            return;

        // 2) Completion date (only if Completed)
        DateTime? completedOnUtc = item.CompletedOnUtc;

        if (newStatus == ComplianceStatus.Completed)
        {
            // Default to existing completion date or today
            var defaultLocal = (completedOnUtc ?? DateTime.UtcNow).ToLocalTime().Date;

            var dateText = await Shell.Current.DisplayPromptAsync(
                "Completion date",
                "Enter date as YYYY-MM-DD",
                accept: "Save",
                cancel: "Cancel",
                initialValue: defaultLocal.ToString("yyyy-MM-dd"),
                keyboard: Keyboard.Text);

            if (dateText == null)
                return; // cancelled

            if (!DateTime.TryParse(dateText, out var parsedLocal))
            {
                await Shell.Current.DisplayAlertAsync("Invalid date", "Please use YYYY-MM-DD", "OK");
                return;
            }

            // Store at midnight local -> convert to UTC
            var localMidnight = parsedLocal.Date;
            completedOnUtc = DateTime.SpecifyKind(localMidnight, DateTimeKind.Local).ToUniversalTime();
        }
        else
        {
            // If not completed, clear completion date
            completedOnUtc = null;
        }

        // 3) Recalculate expiry (next due == expires)
        DateTime? expiresOnUtc = null;
        if (newStatus == ComplianceStatus.Completed &&
            completedOnUtc.HasValue &&
            item.ValidForDays.HasValue &&
            item.ValidForDays.Value > 0)
        {
            expiresOnUtc = completedOnUtc.Value.AddDays(item.ValidForDays.Value);
        }

        await EnsureSessionAsync();

        var ownerId = _session.CurrentOwnerId ?? Guid.Empty;
        if (ownerId == Guid.Empty)
            return;

        var entity = await _driverInductions.GetAsync(
            ownerId,
            SelectedDriver.Id,
            item.WorkSiteId,
            item.RequirementId);

        if (entity == null)
        {
            await Shell.Current.DisplayAlertAsync(
                "Missing",
                "Tap 'Add Induction' to create missing rows.",
                "OK");
            return;
        }

        entity.SetStatus(newStatus);
        entity.SetCompletedOnUtc(completedOnUtc);
        entity.SetExpiresOnUtc(expiresOnUtc);

        await _driverInductions.UpdateAsync(ownerId, SelectedDriver.Id, entity);

        await LoadInductionsAsync();
    }

    #endregion

    #region Add Induction (Create Missing Rows)

    private async Task AddInductionAsync()
    {
        if (SelectedDriver == null)
        {
            await Shell.Current.DisplayAlertAsync("Select a driver", "Choose a driver first.", "OK");
            return;
        }

        await EnsureSessionAsync();

        var ownerId = _session.CurrentOwnerId ?? Guid.Empty;
        if (ownerId == Guid.Empty)
            return;

        var sites = (await _sites.GetAllByOwnerAsync(ownerId)).ToList();
        if (sites.Count == 0)
        {
            await Shell.Current.DisplayAlertAsync(
                "No work sites",
                "Add a work site in Templates first.",
                "OK");
            return;
        }

        var chosen = await Shell.Current.DisplayActionSheet(
            "Choose Work Site",
            "Cancel",
            null,
            sites.Select(s => s.Name).ToArray());

        if (chosen == "Cancel")
            return;

        var site = sites.First(s => s.Name == chosen);

        // Issue date = today (MVP)
        var issueDateLocal = DateTime.Today;

        // Ensure inductions exist for this driver + site, and set issue date
        await _ensurer.EnsureDriverSiteInductionsExistAsync(
            ownerUserId: ownerId,
            driverId: SelectedDriver.Id,
            workSiteId: site.Id,
            issueDateUtc: DateTime.SpecifyKind(issueDateLocal, DateTimeKind.Local).ToUniversalTime());

        await LoadInductionsAsync();
    }

    #endregion

    #region Session Helper

    private async Task EnsureSessionAsync()
    {
        if (!_session.IsAuthenticated)
            await _session.RestoreAsync();
    }

    #endregion
}
