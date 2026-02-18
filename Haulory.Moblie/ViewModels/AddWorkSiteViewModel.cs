using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Application.Interfaces.Services;
using Haulory.Domain.Entities;
using Microsoft.Maui.Controls;

namespace Haulory.Mobile.ViewModels;

public class AddWorkSiteViewModel : BaseViewModel
{
    #region Dependencies

    private readonly ISessionService _session;
    private readonly IWorkSiteRepository _repo;

    #endregion

    #region State

    private bool _isSaving;
    private string _name = string.Empty;

    #endregion

    #region Bindable Properties

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged();
            Refresh();
        }
    }

    public bool CanSave =>
        !_isSaving &&
        _session.IsAuthenticated &&
        !string.IsNullOrWhiteSpace(Name);

    #endregion

    #region Commands

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    #endregion

    #region Constructor

    public AddWorkSiteViewModel(
        ISessionService session,
        IWorkSiteRepository repo)
    {
        _session = session;
        _repo = repo;

        SaveCommand = new Command(async () => await SaveAsync(), () => CanSave);
        CancelCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
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
                await Shell.Current.DisplayAlertAsync(
                    "Not logged in",
                    "Please log in again.",
                    "OK");
                return;
            }

            var site = new WorkSite(ownerId, Name.Trim());
            await _repo.AddAsync(site);

            await Shell.Current.DisplayAlertAsync(
                "Saved",
                "Work site created.",
                "OK");

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                "Save failed",
                ex.Message,
                "OK");
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
