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
        ISessionService sessionService,
        CreateDriverFromUserHandler createDriverFromUserHandler)
    {
        _driverRepository = driverRepository;
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

            // ✅ Restore session on restart
            if (!_sessionService.IsAuthenticated)
                await _sessionService.RestoreAsync();

            var ownerUserId = _sessionService.CurrentUser?.Id ?? Guid.Empty;
            if (!_sessionService.IsAuthenticated || ownerUserId == Guid.Empty)
            {
                _mainDriver = null;
                _isMainComplete = false;
                RaiseGate();
                return;
            }

            // ✅ Load ALL drivers from file (no filtering)
            var all = await _driverRepository.GetAllAsync();

            // ✅ MIGRATE/REPAIR: if main record exists but OwnerUserId was never set, fix it.
            // This is the #1 reason your owned list becomes empty after restart.
            var repaired = false;
            foreach (var d in all)
            {
                // main profile is identified by UserId == current owner
                if (d.UserId.HasValue && d.UserId.Value == ownerUserId && d.OwnerUserId == Guid.Empty)
                {
                    d.EnsureOwner(ownerUserId);
                    await _driverRepository.SaveAsync(d);
                    repaired = true;
                }
            }

            if (repaired)
                all = await _driverRepository.GetAllAsync();

            // ✅ Now re-resolve main from the full list FIRST
            var existingMain = all.FirstOrDefault(d => d.UserId.HasValue && d.UserId.Value == ownerUserId);

            // ✅ If still missing, create it ONCE
            if (existingMain == null)
            {
                var user = _sessionService.CurrentUser!;
                existingMain = await _createDriverFromUserHandler.HandleAsync(
                    new CreateDriverFromUserCommand(
                        ownerUserId,
                        user.FirstName ?? string.Empty,
                        user.LastName ?? string.Empty,
                        user.Email ?? string.Empty
                    )
                );
            }

            // ✅ Load owned drivers (filter)
            var drivers = await _driverRepository.GetAllByOwnerUserIdAsync(ownerUserId);

            // ✅ Make sure main is included in the owned list (if not, add it in-memory so UI shows it)
            if (existingMain != null && drivers.All(d => d.Id != existingMain.Id))
            {
                drivers.Insert(0, existingMain);
            }

            // ✅ Populate UI list
            foreach (var d in drivers
                .OrderByDescending(d => d.UserId.HasValue) // main first
                .ThenBy(d => d.LastName ?? string.Empty)
                .ThenBy(d => d.FirstName ?? string.Empty)
                .ThenBy(d => d.Email ?? string.Empty))
            {
                Drivers.Add(d);
            }

            // ✅ Resolve main driver for gate
            _mainDriver = existingMain;

            // ✅ Gate: completed only if emergency contact fully set
            _isMainComplete = _mainDriver != null &&
                              _mainDriver.EmergencyContact != null &&
                              _mainDriver.EmergencyContact.IsSet;

            RaiseGate();

            // ✅ PROOF POPUP (turn off once confirmed)
            if (DebugGate)
            {
                var ownedCount = all.Count(d => d.OwnerUserId == ownerUserId);
                var mainCount = all.Count(d => d.UserId.HasValue && d.UserId.Value == ownerUserId);

                await Shell.Current.DisplayAlertAsync(
                    "Driver Store Proof",
                    $"OwnerUserId: {ownerUserId}\n" +
                    $"All drivers in file: {all.Count}\n" +
                    $"Owned drivers (OwnerUserId match): {ownedCount}\n" +
                    $"Main profile (UserId match): {mainCount}\n" +
                    $"UI Drivers count: {Drivers.Count}\n" +
                    $"Gate complete: {_isMainComplete}",
                    "OK");
            }
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
 