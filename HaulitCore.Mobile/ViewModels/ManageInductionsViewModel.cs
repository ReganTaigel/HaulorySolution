using HaulitCore.Application.Features.Incductions;
using HaulitCore.Application.Interfaces.Repositories;
using HaulitCore.Application.Interfaces.Services;
using HaulitCore.Domain.Entities;
using HaulitCore.Mobile.Features;
using HaulitCore.Mobile.Services;
using HaulitCore.Mobile.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace HaulitCore.Mobile.ViewModels;

public class ManageInductionsViewModel : BaseViewModel
{
    private readonly ISessionService _session;
    private readonly IDriverInductionRepository _driverInductions;
    private readonly IComplianceEnsurer _ensurer;
    private readonly IWorkSiteRepository _sites;
    private readonly DriversApiService _driversApiService;

    private DriverPickerItem? _selectedDriver;

    public ObservableCollection<DriverPickerItem> Drivers { get; } = new();
    public ObservableCollection<DriverInductionListItemDto> Inductions { get; } = new();

    public bool IsInductionsVisible => IsFeatureVisible(AppFeature.Inductions);
    public bool IsInductionsEnabled => IsFeatureEnabled(AppFeature.Inductions);

    public DriverPickerItem? SelectedDriver
    {
        get => _selectedDriver;
        set
        {
            if (!SetProperty(ref _selectedDriver, value))
                return;

            _ = LoadInductionsAsync();
        }
    }

    public ICommand RefreshCommand { get; }
    public ICommand GoToTemplatesCommand { get; }
    public ICommand AddInductionCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand UploadProofCommand { get; }
    public ICommand OpenProofCommand { get; }
    public ICommand RemoveProofCommand { get; }

    public ManageInductionsViewModel(
        ISessionService session,
        IDriverInductionRepository driverInductions,
        IWorkSiteRepository sites,
        IComplianceEnsurer ensurer,
        DriversApiService driversApiService,
        IFeatureAccessService featureAccessService)
        : base(featureAccessService)
    {
        _session = session;
        _driverInductions = driverInductions;
        _sites = sites;
        _ensurer = ensurer;
        _driversApiService = driversApiService;

        RefreshCommand = new Command(async () => await LoadAsync());

        GoToTemplatesCommand = new Command(async () =>
        {
            if (!await EnsureFeatureEnabledAsync(AppFeature.Inductions))
                return;

            await Shell.Current.GoToAsync(nameof(InductionTemplatesPage));
        });

        AddInductionCommand = new Command(async () => await AddInductionAsync());

        EditCommand = new Command<DriverInductionListItemDto>(async item =>
        {
            if (item == null || SelectedDriver == null)
                return;

            await EditAsync(item);
        });

        UploadProofCommand = new Command<DriverInductionListItemDto>(async item =>
        {
            if (item == null || SelectedDriver == null)
                return;

            await UploadProofAsync(item);
        });

        OpenProofCommand = new Command<DriverInductionListItemDto>(async item =>
        {
            if (item == null || string.IsNullOrWhiteSpace(item.EvidenceUrl))
                return;

            if (!await EnsureFeatureEnabledAsync(AppFeature.Inductions))
                return;

            await Launcher.Default.OpenAsync(item.EvidenceUrl);
        });

        RemoveProofCommand = new Command<DriverInductionListItemDto>(async item =>
        {
            if (item == null || SelectedDriver == null)
                return;

            await RemoveProofAsync(item);
        });
    }

    public async Task LoadAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            Drivers.Clear();
            Inductions.Clear();

            if (!IsFeatureEnabled(AppFeature.Inductions))
            {
                RefreshFeatureBindings();
                return;
            }

            await EnsureSessionAsync();

            var ownerId = _session.CurrentOwnerId ?? Guid.Empty;
            if (ownerId == Guid.Empty)
            {
                RefreshFeatureBindings();
                return;
            }

            var drivers = await _driversApiService.GetDriversAsync();

            foreach (var d in drivers
                         .OrderBy(x => x.FirstName)
                         .ThenBy(x => x.LastName))
            {
                Drivers.Add(new DriverPickerItem
                {
                    Id = d.Id,
                    DisplayName = $"{d.FirstName} {d.LastName}".Trim()
                });
            }

            if (SelectedDriver == null && Drivers.Count > 0)
                SelectedDriver = Drivers[0];

            RefreshFeatureBindings();
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

    private async Task LoadInductionsAsync()
    {
        try
        {
            Inductions.Clear();

            if (!IsFeatureEnabled(AppFeature.Inductions))
            {
                RefreshFeatureBindings();
                return;
            }

            if (SelectedDriver == null)
                return;

            await EnsureSessionAsync();

            var ownerId = _session.CurrentOwnerId ?? Guid.Empty;
            if (ownerId == Guid.Empty)
                return;

            var list = await _driverInductions.GetListItemsByDriverAsync(ownerId, SelectedDriver.Id);

            foreach (var x in list)
                Inductions.Add(x);

            RefreshFeatureBindings();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Load failed", ex.Message, "OK");
        }
    }

    private async Task EditAsync(DriverInductionListItemDto item)
    {
        if (!await EnsureFeatureEnabledAsync(AppFeature.Inductions))
            return;

        if (SelectedDriver == null)
            return;

        var statusText = await Shell.Current.DisplayActionSheetAsync(
            "Set Status",
            "Cancel",
            null,
            ComplianceStatus.NotStarted.ToString(),
            ComplianceStatus.InProgress.ToString(),
            ComplianceStatus.Completed.ToString(),
            ComplianceStatus.Expired.ToString());

        if (statusText == "Cancel" || !Enum.TryParse(statusText, out ComplianceStatus newStatus))
            return;

        DateTime? completedOnUtc = item.CompletedOnUtc;

        if (newStatus == ComplianceStatus.Completed)
        {
            var defaultLocal = (completedOnUtc ?? DateTime.UtcNow).ToLocalTime().Date;

            var dateText = await Shell.Current.DisplayPromptAsync(
                "Completion date",
                "Enter date as YYYY-MM-DD",
                accept: "Save",
                cancel: "Cancel",
                initialValue: defaultLocal.ToString("yyyy-MM-dd"),
                keyboard: Keyboard.Text);

            if (dateText == null)
                return;

            if (!DateTime.TryParse(dateText, out var parsedLocal))
            {
                await Shell.Current.DisplayAlertAsync("Invalid date", "Please use YYYY-MM-DD", "OK");
                return;
            }

            var localMidnight = parsedLocal.Date;
            completedOnUtc = DateTime.SpecifyKind(localMidnight, DateTimeKind.Local).ToUniversalTime();
        }
        else
        {
            completedOnUtc = null;
        }

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
            await Shell.Current.DisplayAlertAsync("Missing", "Tap 'Add Induction' to create missing rows.", "OK");
            return;
        }

        entity.SetStatus(newStatus);
        entity.SetCompletedOnUtc(completedOnUtc);
        entity.SetExpiresOnUtc(expiresOnUtc);

        await _driverInductions.UpdateAsync(ownerId, SelectedDriver.Id, entity);

        await LoadInductionsAsync();
    }

    private async Task UploadProofAsync(DriverInductionListItemDto item)
    {
        try
        {
            if (!await EnsureFeatureEnabledAsync(AppFeature.Inductions))
                return;

            if (SelectedDriver == null)
                return;

            var fileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Android, new[] { "application/pdf", "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "image/*" } },
                { DevicePlatform.iOS, new[] { "com.adobe.pdf", "public.image", "com.microsoft.word.doc", "org.openxmlformats.wordprocessingml.document" } },
                { DevicePlatform.WinUI, new[] { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png" } },
                { DevicePlatform.MacCatalyst, new[] { "pdf", "doc", "docx", "jpg", "jpeg", "png" } }
            });

            var file = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Choose proof file",
                FileTypes = fileTypes
            });

            if (file == null)
                return;

            if (string.IsNullOrWhiteSpace(file.FullPath))
            {
                await Shell.Current.DisplayAlertAsync("File error", "The selected file path could not be read.", "OK");
                return;
            }

            await _driversApiService.UploadInductionEvidenceAsync(
                SelectedDriver.Id,
                item.WorkSiteId,
                item.RequirementId,
                file.FullPath);

            await LoadInductionsAsync();

            await Shell.Current.DisplayAlertAsync("Uploaded", "Proof file uploaded.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Upload failed", ex.Message, "OK");
        }
    }

    private async Task RemoveProofAsync(DriverInductionListItemDto item)
    {
        try
        {
            if (!await EnsureFeatureEnabledAsync(AppFeature.Inductions))
                return;

            if (SelectedDriver == null || !item.HasEvidence)
                return;

            var confirm = await Shell.Current.DisplayAlertAsync(
                "Remove proof",
                "Remove this proof document?",
                "Remove",
                "Cancel");

            if (!confirm)
                return;

            await _driversApiService.DeleteInductionEvidenceAsync(
                SelectedDriver.Id,
                item.WorkSiteId,
                item.RequirementId);

            await LoadInductionsAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Remove failed", ex.Message, "OK");
        }
    }

    private async Task AddInductionAsync()
    {
        if (!await EnsureFeatureEnabledAsync(AppFeature.Inductions))
            return;

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
            await Shell.Current.DisplayAlertAsync("No work sites", "Add a work site in Templates first.", "OK");
            return;
        }

        var chosen = await Shell.Current.DisplayActionSheetAsync(
            "Choose Work Site",
            "Cancel",
            null,
            sites.Select(s => s.Name).ToArray());

        if (chosen == "Cancel")
            return;

        var site = sites.First(s => s.Name == chosen);

        var issueDateLocal = DateTime.Today;

        await _ensurer.EnsureDriverSiteInductionsExistAsync(
            ownerUserId: ownerId,
            driverId: SelectedDriver.Id,
            workSiteId: site.Id,
            issueDateUtc: DateTime.SpecifyKind(issueDateLocal, DateTimeKind.Local).ToUniversalTime());

        await LoadInductionsAsync();
    }

    private async Task EnsureSessionAsync()
    {
        if (!_session.IsAuthenticated)
            await _session.RestoreAsync();
    }

    private void RefreshFeatureBindings()
    {
        OnPropertyChanged(nameof(IsInductionsVisible));
        OnPropertyChanged(nameof(IsInductionsEnabled));
    }
}