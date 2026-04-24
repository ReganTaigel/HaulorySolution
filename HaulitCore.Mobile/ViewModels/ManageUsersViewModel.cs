using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using HaulitCore.Application.Features.Users;
using HaulitCore.Application.Interfaces.Services;
using HaulitCore.Domain.Entities;
using Microsoft.Maui.Controls;

namespace HaulitCore.Mobile.ViewModels;

public class ManageUsersViewModel : BaseViewModel
{
    private readonly GetSubUsersHandler _getSubUsers;
    private readonly CreateSubUserHandler _createSubUser;
    private readonly ISessionService _session;

    public ObservableCollection<UserAccount> SubUsers { get; } = new();

    // Form fields
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public ICommand RefreshCommand { get; }
    public ICommand CreateCommand { get; }

    public ManageUsersViewModel(
        GetSubUsersHandler getSubUsers,
        CreateSubUserHandler createSubUser,
        ISessionService session)
    {
        _getSubUsers = getSubUsers;
        _createSubUser = createSubUser;
        _session = session;

        RefreshCommand = new Command(async () => await LoadAsync());
        CreateCommand = new Command(async () => await CreateAsync());
    }

    public async Task LoadAsync()
    {
        SubUsers.Clear();

        // Ensure session restored
        if (!_session.IsAuthenticated)
            await _session.RestoreAsync();

        var ownerId = _session.CurrentOwnerId ?? Guid.Empty;
        if (ownerId == Guid.Empty)
            return;

        var users = await _getSubUsers.HandleAsync(ownerId);

        foreach (var u in users.OrderBy(x => x.LastName).ThenBy(x => x.FirstName))
            SubUsers.Add(u);
    }

    private async Task CreateAsync()
    {
        // Ensure session restored
        if (!_session.IsAuthenticated)
            await _session.RestoreAsync();

        var requestorAccountId = _session.CurrentAccountId ?? Guid.Empty;
        var ownerId = _session.CurrentOwnerId ?? Guid.Empty;

        if (requestorAccountId == Guid.Empty || ownerId == Guid.Empty)
            return;

        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            await Shell.Current.DisplayAlertAsync("Missing info", "Email and password are required.", "OK");
            return;
        }

        var created = await _createSubUser.HandleAsync(new CreateSubUserCommand(
            RequestorAccountId: requestorAccountId,
            OwnerMainUserId: ownerId,
            FirstName: FirstName,
            LastName: LastName,
            Email: Email,
            Password: Password
        ));

        if (created == null)
        {
            await Shell.Current.DisplayAlertAsync(
                "Not created",
                "Could not create user (email may already exist, or you don’t have permission).",
                "OK");
            return;
        }

        // Clear form
        FirstName = LastName = Email = Password = string.Empty;
        OnPropertyChanged(nameof(FirstName));
        OnPropertyChanged(nameof(LastName));
        OnPropertyChanged(nameof(Email));
        OnPropertyChanged(nameof(Password));

        await LoadAsync();
    }
}