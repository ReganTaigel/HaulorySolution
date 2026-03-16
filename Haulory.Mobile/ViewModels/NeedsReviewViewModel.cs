using Haulory.Application.Interfaces.Services;
using Haulory.Contracts.Jobs;
using Haulory.Mobile.Features;
using Haulory.Mobile.Services;
using Haulory.Mobile.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Haulory.Mobile.ViewModels;

public class NeedsReviewViewModel : BaseViewModel
{
    private readonly JobsApiService _jobsApiService;
    private readonly ISessionService _session;

    private bool _isLoading;

    public ObservableCollection<JobListItemViewModel> Jobs { get; } = new();

    public bool IsMainUser =>
        _session.CurrentAccountId.HasValue &&
        _session.CurrentOwnerId.HasValue &&
        _session.CurrentOwnerId.Value == _session.CurrentAccountId.Value;

    public bool IsJobsVisible => IsFeatureVisible(AppFeature.Jobs);
    public bool IsJobsEnabled => IsFeatureEnabled(AppFeature.Jobs);

    public ICommand RefreshCommand { get; }
    public ICommand OpenReviewCommand { get; }

    public NeedsReviewViewModel(
        JobsApiService jobsApiService,
        ISessionService session,
        IFeatureAccessService featureAccessService)
        : base(featureAccessService)
    {
        _jobsApiService = jobsApiService;
        _session = session;

        RefreshCommand = new Command(async () => await LoadAsync());

        OpenReviewCommand = new Command<JobListItemViewModel>(async item =>
        {
            if (item?.Job == null)
                return;

            await Shell.Current.GoToAsync(nameof(NewJobPage), new Dictionary<string, object>
            {
                ["jobId"] = item.Job.Id,
                ["reviewOnly"] = true
            });
        });
    }

    public async Task LoadAsync()
    {
        if (_isLoading)
            return;

        _isLoading = true;

        try
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
        }
        finally
        {
            _isLoading = false;
        }
    }
}