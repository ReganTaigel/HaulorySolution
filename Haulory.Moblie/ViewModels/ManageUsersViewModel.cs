using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Haulory.Application.Features.Users;
using Haulory.Application.Interfaces.Services;
using Haulory.Domain.Entities;
using Microsoft.Maui.Controls;

namespace Haulory.Mobile.ViewModels;

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

        var ownerId = _session.CurrentOwnerId ?? Guid.Empty;
        if (ownerId == Guid.Empty)
            return;

        var users = await _getSubUsers.HandleAsync(ownerId);
        foreach (var u in users.OrderBy(x => x.LastName).ThenBy(x => x.FirstName))
            SubUsers.Add(u);
    }

    private async Task CreateAsync()
    {
        var ownerId = _session.CurrentOwnerId ?? Guid.Empty;
        if (ownerId == Guid.Empty)
            return;

        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            await Shell.Current.DisplayAlertAsync("Missing info", "Email and password are required.", "OK");
            return;
        }

        var created = await _createSubUser.HandleAsync(new CreateSubUserCommand(
            OwnerMainUserId: ownerId,
            FirstName: FirstName,
            LastName: LastName,
            Email: Email,
            Password: Password
        ));

        if (created == null)
        {
            await Shell.Current.DisplayAlertAsync("Not created", "Could not create user (email may already exist).", "OK");
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