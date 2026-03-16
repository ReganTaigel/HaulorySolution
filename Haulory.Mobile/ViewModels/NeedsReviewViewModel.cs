using Haulory.Application.Interfaces.Services;
using Haulory.Contracts.Jobs;
using Haulory.Mobile.Diagnostics;
using Haulory.Mobile.Features;
using Haulory.Mobile.Services;
using Haulory.Mobile.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Haulory.Mobile.ViewModels;

public class NeedsReviewViewModel : BaseViewModel
{
    #region Dependencies

    private readonly JobsApiService _jobsApiService;
    private readonly ISessionService _session;
    private readonly ICrashLogger _crashLogger;

    #endregion

    #region State

    private bool _isLoading;

    #endregion

    #region Collections

    public ObservableCollection<JobListItemViewModel> Jobs { get; } = new();

    #endregion

    #region Feature Access

    public bool IsMainUser =>
        _session.CurrentAccountId.HasValue &&
        _session.CurrentOwnerId.HasValue &&
        _session.CurrentOwnerId.Value == _session.CurrentAccountId.Value;

    public bool IsJobsVisible => IsFeatureVisible(AppFeature.Jobs);
    public bool IsJobsEnabled => IsFeatureEnabled(AppFeature.Jobs);

    #endregion

    #region Commands

    public ICommand RefreshCommand { get; }
    public ICommand OpenReviewCommand { get; }

    #endregion

    #region Constructor

    public NeedsReviewViewModel(
        JobsApiService jobsApiService,
        ISessionService session,
        IFeatureAccessService featureAccessService,
        ICrashLogger crashLogger)
        : base(featureAccessService)
    {
        _jobsApiService = jobsApiService;
        _session = session;
        _crashLogger = crashLogger;

        RefreshCommand = new Command(async () => await LoadAsync());

        OpenReviewCommand = new Command<JobListItemViewModel>(async item =>
        {
            if (item?.Job == null)
                return;

            await SafeRunner.RunAsync(
                async () =>
                {
                    await Shell.Current.GoToAsync(nameof(NewJobPage), new Dictionary<string, object>
                    {
                        ["jobId"] = item.Job.Id,
                        ["reviewOnly"] = true
                    });
                },
                _crashLogger,
                "NeedsReviewViewModel.OpenReviewCommand",
                nameof(NeedsReviewPage),
                metadataJson: $"{{\"JobId\":\"{item.Job.Id}\"}}");
        });
    }

    #endregion

    #region Public Methods

    public async Task LoadAsync()
    {
        if (_isLoading)
            return;

        _isLoading = true;

        try
        {
            await SafeRunner.RunAsync(
                async () =>
                {
                    Jobs.Clear();

                    if (!_session.IsAuthenticated)
                        await _session.RestoreAsync();

                    if (!IsMainUser)
                        return;

                    var jobs = await _jobsApiService.GetNeedsReviewJobsAsync();

                    foreach (var job in jobs.OrderByDescending(x => x.DeliveredAtUtc))
                    {
                        var driverName = "—";
                        var truckDisplay = "—";
                        var showPricing = true;
                        var canShowSignDelivery = false;

                        Jobs.Add(new JobListItemViewModel(
                            job,
                            driverName,
                            truckDisplay,
                            showPricing,
                            canShowSignDelivery));
                    }
                },
                _crashLogger,
                "NeedsReviewViewModel.LoadAsync",
                nameof(NeedsReviewPage),
                onError: async ex =>
                {
                    await Shell.Current.DisplayAlertAsync("Load failed", ex.Message, "OK");
                });
        }
        finally
        {
            _isLoading = false;
        }
    }

    #endregion
}