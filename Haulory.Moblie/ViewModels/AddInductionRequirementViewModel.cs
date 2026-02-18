using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Application.Interfaces.Services;
using Haulory.Domain.Entities;
using Microsoft.Maui.Controls;

namespace Haulory.Mobile.ViewModels;

[QueryProperty(nameof(WorkSiteId), "workSiteId")]
public class AddInductionRequirementViewModel : BaseViewModel
{
    #region Dependencies

    private readonly ISessionService _session;
    private readonly IWorkSiteRepository _sites;
    private readonly IInductionRequirementRepository _reqs;

    #endregion

    #region State

    private bool _isSaving;
    private string _workSiteId = string.Empty;

    private string _workSiteName = string.Empty;
    private string _title = string.Empty;
    private string _validForDaysText = string.Empty;
    private string _ppeRequired = string.Empty;

    #endregion

    #region Bindable Properties

    public string WorkSiteId
    {
        get => _workSiteId;
        set
        {
            _workSiteId = value;
            OnPropertyChanged();

            // Fire-and-forget load, safe because it handles exceptions internally
            _ = LoadSiteAsync();
        }
    }

    public string WorkSiteName
    {
        get => _workSiteName;
        set
        {
            _workSiteName = value;
            OnPropertyChanged();
        }
    }

    public string Title
    {
        get => _title;
        set
        {
            _title = value;
            OnPropertyChanged();
            Refresh();
        }
    }

    // Kept as text to avoid numeric input UX pain
    public string ValidForDaysText
    {
        get => _validForDaysText;
        set
        {
            _validForDaysText = value;
            OnPropertyChanged();
            Refresh();
        }
    }

    // Optional PPE list (e.g. "Hi-vis, Hard hat, Steel caps")
    public string PpeRequired
    {
        get => _ppeRequired;
        set
        {
            _ppeRequired = value;
            OnPropertyChanged();
            Refresh();
        }
    }

    public bool CanSave
    {
        get
        {
            if (_isSaving) return false;
            if (!_session.IsAuthenticated) return false;
            if (!Guid.TryParse(WorkSiteId, out _)) return false;
            if (string.IsNullOrWhiteSpace(Title)) return false;

            // Optional: if filled, must be valid int > 0
            if (!string.IsNullOrWhiteSpace(ValidForDaysText))
            {
                if (!int.TryParse(ValidForDaysText.Trim(), out var days)) return false;
                if (days <= 0) return false;
            }

            return true;
        }
    }

    #endregion

    #region Commands

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    #endregion

    #region Constructor

    public AddInductionRequirementViewModel(
        ISessionService session,
        IWorkSiteRepository sites,
        IInductionRequirementRepository reqs)
    {
        _session = session;
        _sites = sites;
        _reqs = reqs;

        SaveCommand = new Command(async () => await SaveAsync(), () => CanSave);
        CancelCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
    }

    #endregion

    #region Data Loading

    private async Task LoadSiteAsync()
    {
        try
        {
            // Ensure session is restored
            if (!_session.IsAuthenticated)
                await _session.RestoreAsync();

            var ownerId = _session.CurrentAccountId ?? Guid.Empty;
            if (ownerId == Guid.Empty)
                return;

            if (!Guid.TryParse(WorkSiteId, out var siteId))
                return;

            var site = await _sites.GetByIdAsync(ownerId, siteId);
            WorkSiteName = site?.Name ?? string.Empty;
        }
        catch
        {
            // Keep silent; page can still work without the name
        }
        finally
        {
            Refresh();
        }
    }

    #endregion

    #region Save Flow

    private async Task SaveAsync()
    {
        if (!CanSave)
            return;

        try
        {
            _isSaving = true;
            Refresh();

            // Ensure session is restored
            if (!_session.IsAuthenticated)
                await _session.RestoreAsync();

            var ownerId = _session.CurrentAccountId ?? Guid.Empty;
            if (ownerId == Guid.Empty)
            {
                await Shell.Current.DisplayAlertAsync("Not logged in", "Please log in again.", "OK");
                return;
            }

            if (!Guid.TryParse(WorkSiteId, out var siteId))
                return;

            int? validDays = null;
            if (!string.IsNullOrWhiteSpace(ValidForDaysText))
                validDays = int.Parse(ValidForDaysText.Trim());

            var req = new InductionRequirement(
                ownerUserId: ownerId,
                workSiteId: siteId,
                title: Title.Trim(),
                validForDays: validDays,
                ppeRequired: string.IsNullOrWhiteSpace(PpeRequired) ? null : PpeRequired.Trim()
            );

            await _reqs.AddAsync(req);

            await Shell.Current.DisplayAlertAsync("Saved", "Requirement created.", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Save failed", ex.Message, "OK");
        }
        finally
        {
            _isSaving = false;
            Refresh();
        }
    }

    #endregion

    #region UI Helpers

    private void Refresh()
    {
        OnPropertyChanged(nameof(CanSave));
        (SaveCommand as Command)?.ChangeCanExecute();
    }

    #endregion
}
