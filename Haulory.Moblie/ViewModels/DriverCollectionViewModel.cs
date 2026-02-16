using Haulory.Application.Features.Drivers;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Application.Interfaces.Services;
using Haulory.Domain.Entities;
using Haulory.Mobile.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Haulory.Mobile.ViewModels;

public class DriverCollectionViewModel : BaseViewModel
{
    private readonly IDriverRepository _driverRepository;
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly ISessionService _sessionService;
    private readonly CreateDriverFromUserHandler _createDriverFromUserHandler;

    // Toggle true while debugging.
    private const bool DebugGate = true;

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set { _isBusy = value; OnPropertyChanged(); }
    }

    private Driver? _mainDriver;
    private bool _isMainComplete;

    public bool ShowContinue => !_isMainComplete;
    public bool ShowAddDriver => _isMainComplete;

    public ObservableCollection<Driver> Drivers { get; } = new();

    public ICommand AddDriverCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand ContinueCommand { get; }

    public DriverCollectionViewModel(
        IDriverRepository driverRepository,
        IUserAccountRepository userAccountRepository,
        ISessionService sessionService,
        CreateDriverFromUserHandler createDriverFromUserHandler)
    {
        _driverRepository = driverRepository;
        _userAccountRepository = userAccountRepository;
        _sessionService = sessionService;
        _createDriverFromUserHandler = createDriverFromUserHandler;

        AddDriverCommand = new Command(async () =>
            await Shell.Current.GoToAsync(nameof(NewDriverPage)));

        ContinueCommand = new Command(async () =>
        {
            if (_mainDriver == null) return;
            await Shell.Current.GoToAsync($"{nameof(EditDriverPage)}?driverId={_mainDriver.Id}");
        });

        RefreshCommand = new Command(async () => await LoadAsync());
    }

    public async Task LoadAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            Drivers.Clear();

            // Restore session on restart
            if (!_sessionService.IsAuthenticated)
                await _sessionService.RestoreAsync();

            var ownerUserId = _sessionService.CurrentAccountId ?? Guid.Empty;
            if (!_sessionService.IsAuthenticated || ownerUserId == Guid.Empty)
            {
                _mainDriver = null;
                _isMainComplete = false;
                RaiseGate();
                return;
            }

            // Load owned drivers
            var drivers = await _driverRepository.GetAllByOwnerUserIdAsync(ownerUserId);

            // Resolve main driver (the one linked to the current account)
            var existingMain = drivers.FirstOrDefault(d => d.UserId.HasValue && d.UserId.Value == ownerUserId);

            // If missing, create it once from the UserAccount record
            if (existingMain == null)
            {
                var account = await _userAccountRepository.GetByIdAsync(ownerUserId);
                if (account == null)
                {
                    _mainDriver = null;
                    _isMainComplete = false;
                    RaiseGate();
                    return;
                }

                existingMain = await _createDriverFromUserHandler.HandleAsync(
                    new CreateDriverFromUserCommand(
                        ownerUserId,
                        account.FirstName ?? string.Empty,
                        account.LastName ?? string.Empty,
                        account.Email ?? string.Empty
                    )
                );

                // Reload owned list after creation
                drivers = await _driverRepository.GetAllByOwnerUserIdAsync(ownerUserId);
            }

            // Ensure main appears in list (if handler returned it but repo query didn't include yet)
            if (existingMain != null && drivers.All(d => d.Id != existingMain.Id))
                drivers.Insert(0, existingMain);

            foreach (var d in drivers
                .OrderByDescending(d => d.UserId.HasValue) // main first
                .ThenBy(d => d.LastName ?? string.Empty)
                .ThenBy(d => d.FirstName ?? string.Empty)
                .ThenBy(d => d.Email ?? string.Empty))
            {
                Drivers.Add(d);
            }

            _mainDriver = existingMain;

            _isMainComplete = _mainDriver != null &&
                              _mainDriver.EmergencyContact != null &&
                              _mainDriver.EmergencyContact.IsSet;

            RaiseGate();

        }
        finally
        {
            IsBusy = false;
        }
    }

    private void RaiseGate()
    {
        OnPropertyChanged(nameof(ShowContinue));
        OnPropertyChanged(nameof(ShowAddDriver));
    }
}
