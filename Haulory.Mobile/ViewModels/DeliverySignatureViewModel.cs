using Haulory.Application.Interfaces.Services;
using Haulory.Contracts.Jobs;
using Haulory.Mobile.Diagnostics;
using Haulory.Mobile.Services;
using Haulory.Mobile.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System.Text.Json;
using System.Windows.Input;

namespace Haulory.Mobile.ViewModels;

[QueryProperty(nameof(JobId), "jobId")]
public class DeliverySignatureViewModel : BaseViewModel
{
    #region Dependencies

    private readonly JobsApiService _jobsApiService;
    private readonly ISessionService _session;
    private readonly IAppEventBus? _events;
    private readonly ICrashLogger _crashLogger;

    #endregion

    #region State

    private JobDto? _job;
    private bool _isSaving;

    private string _receiverName = string.Empty;
    private string _statusMessage = string.Empty;

    private Guid _jobId;

    private string? _damageNotes;
    private int? _waitTimeMinutes;

    #endregion

    #region Bindable Properties - Job Details

    public string ReferenceNumber => _job?.ReferenceNumber ?? string.Empty;
    public string PickupCompany => _job?.PickupCompany ?? string.Empty;
    public string DeliveryCompany => _job?.DeliveryCompany ?? string.Empty;
    public string DeliveryAddress => _job?.DeliveryAddress ?? string.Empty;
    public string LoadDescription => _job?.LoadDescription ?? string.Empty;

    public bool IsDelivered => _job?.IsDelivered ?? false;

    #endregion

    #region Bindable Properties - Receiver + Status

    public string ReceiverName
    {
        get => _receiverName;
        set
        {
            if (_receiverName == value) return;
            _receiverName = value;
            OnPropertyChanged();
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            if (_statusMessage == value) return;
            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    public int? WaitTimeMinutes
    {
        get => _waitTimeMinutes;
        set
        {
            if (_waitTimeMinutes == value) return;
            _waitTimeMinutes = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(WaitTimeDisplay));
            OnPropertyChanged(nameof(HasPreDeliveryInfo));
        }
    }

    public string? DamageNotes
    {
        get => _damageNotes;
        set
        {
            if (_damageNotes == value) return;
            _damageNotes = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DamageNotesDisplay));
            OnPropertyChanged(nameof(HasPreDeliveryInfo));
        }
    }

    public bool HasPreDeliveryInfo =>
        WaitTimeMinutes.HasValue || !string.IsNullOrWhiteSpace(DamageNotes);

    public string WaitTimeDisplay =>
        WaitTimeMinutes.HasValue
            ? $"{WaitTimeMinutes.Value} minutes"
            : "None recorded";

    public string DamageNotesDisplay =>
        string.IsNullOrWhiteSpace(DamageNotes)
            ? "None recorded"
            : DamageNotes!;

    #endregion

    #region Signature Data

    private readonly List<List<PointF>> _strokes = new();
    private List<PointF>? _currentStroke;

    public SignatureDrawable SignatureDrawable { get; }

    #endregion

    #region Commands

    public ICommand ClearSignatureCommand { get; }
    public ICommand SaveCommand { get; }

    #endregion

    #region Query Params

    public string JobId
    {
        get => _jobId.ToString();
        set
        {
            if (!Guid.TryParse(value, out var id))
                return;

            _jobId = id;
            _ = LoadJobSafeAsync();
        }
    }

    #endregion

    #region Constructor

    public DeliverySignatureViewModel(
        JobsApiService jobsApiService,
        ISessionService session,
        ICrashLogger crashLogger,
        IAppEventBus? events = null)
    {
        _jobsApiService = jobsApiService;
        _session = session;
        _crashLogger = crashLogger;
        _events = events;

        SignatureDrawable = new SignatureDrawable(_strokes);

        ClearSignatureCommand = new Command(() =>
        {
            _strokes.Clear();
            _currentStroke = null;
            SignatureDrawable.InvalidateRequested?.Invoke();
            StatusMessage = "Signature cleared.";
        });

        SaveCommand = new Command(async () => await SaveAsync(), () => !_isSaving);
    }

    #endregion

    #region Signature Section Expand

    private bool _isSignatureExpanded = true;

    public bool IsSignatureExpanded
    {
        get => _isSignatureExpanded;
        set
        {
            if (_isSignatureExpanded == value)
                return;

            _isSignatureExpanded = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SignatureExpandIcon));
            OnPropertyChanged(nameof(SignatureExpandText));
        }
    }

    public string SignatureExpandIcon => IsSignatureExpanded ? "▲" : "▼";

    public string SignatureExpandText =>
        IsSignatureExpanded ? "Hide signature" : "Show signature";

    public ICommand ToggleSignatureCommand => new Command(() =>
    {
        IsSignatureExpanded = !IsSignatureExpanded;
    });

    #endregion

    #region Load

    private async Task LoadJobSafeAsync()
    {
        await SafeRunner.RunAsync(
            async () => await LoadJobAsync(),
            _crashLogger,
            "DeliverySignatureViewModel.LoadJobSafeAsync",
            nameof(DeliverySignaturePage),
            metadataJson: $"{{\"JobId\":\"{_jobId}\"}}",
            onError: async ex =>
            {
                StatusMessage = $"Load failed: {ex.Message}";
                await Task.CompletedTask;
            });
    }

    private async Task LoadJobAsync()
    {
        StatusMessage = "Loading job...";

        _job = await _jobsApiService.GetJobByIdAsync(_jobId);

        if (_job == null)
        {
            StatusMessage = "Job not found.";
            RaiseJobPropertiesChanged();
            return;
        }

        if (!string.IsNullOrWhiteSpace(_job.ReceiverName))
            ReceiverName = _job.ReceiverName;

        WaitTimeMinutes = _job.WaitTimeMinutes;
        DamageNotes = _job.DamageNotes;

        RaiseJobPropertiesChanged();

        StatusMessage = _job.IsDelivered
            ? "This job has already been delivered."
            : "Review details, collect signature, and save delivery.";
    }

    private void RaiseJobPropertiesChanged()
    {
        OnPropertyChanged(nameof(ReferenceNumber));
        OnPropertyChanged(nameof(PickupCompany));
        OnPropertyChanged(nameof(DeliveryCompany));
        OnPropertyChanged(nameof(DeliveryAddress));
        OnPropertyChanged(nameof(LoadDescription));
        OnPropertyChanged(nameof(IsDelivered));
        OnPropertyChanged(nameof(WaitTimeDisplay));
        OnPropertyChanged(nameof(DamageNotesDisplay));
        OnPropertyChanged(nameof(HasPreDeliveryInfo));
    }

    #endregion

    #region Touch Methods

    public void StartStroke(PointF point)
    {
        _currentStroke = new List<PointF> { point };
        _strokes.Add(_currentStroke);
        SignatureDrawable.InvalidateRequested?.Invoke();
    }

    public void DragStroke(PointF point)
    {
        _currentStroke?.Add(point);
        SignatureDrawable.InvalidateRequested?.Invoke();
    }

    public void EndStroke()
    {
        _currentStroke = null;
        SignatureDrawable.InvalidateRequested?.Invoke();
    }

    #endregion

    #region Save Flow

    private async Task SaveAsync()
    {
        if (_isSaving)
            return;

        _isSaving = true;
        ((Command)SaveCommand).ChangeCanExecute();

        try
        {
            if (_job == null)
            {
                StatusMessage = "Job not loaded.";
                return;
            }

            if (_job.IsDelivered)
            {
                await Shell.Current.DisplayAlertAsync(
                    "Already Delivered",
                    "This job has already been signed off.",
                    "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(ReceiverName))
            {
                await Shell.Current.DisplayAlertAsync(
                    "Missing Info",
                    "Please enter receiver name.",
                    "OK");
                return;
            }

            if (!HasSignature())
            {
                await Shell.Current.DisplayAlertAsync(
                    "Missing Signature",
                    "Please capture a signature before saving.",
                    "OK");
                return;
            }

            var ownerUserId = _session.CurrentOwnerId ?? Guid.Empty;
            var deliveredByUserId = _session.CurrentAccountId ?? Guid.Empty;

            if (ownerUserId == Guid.Empty || deliveredByUserId == Guid.Empty)
            {
                await Shell.Current.DisplayAlertAsync("Not logged in", "Please log in again.", "OK");
                return;
            }

            await SafeRunner.RunAsync(
                async () =>
                {
                    var signatureJson = BuildSignatureJson();

                    var request = new CompleteJobRequest
                    {
                        ReceiverName = ReceiverName.Trim(),
                        SignatureJson = signatureJson,
                        WaitTimeMinutes = WaitTimeMinutes,
                        DamageNotes = string.IsNullOrWhiteSpace(DamageNotes) ? null : DamageNotes.Trim()
                    };

                    await _jobsApiService.CompleteJobAsync(_job.Id, request);

                    if (_events != null)
                        await _events.PublishAsync(new JobCompletedEvent(_job.Id));

                    var requiresReview =
                        (WaitTimeMinutes.HasValue && WaitTimeMinutes.Value > 0) ||
                        !string.IsNullOrWhiteSpace(DamageNotes);

                    var msg = requiresReview
                        ? "Delivery saved. Exceptions recorded—Main user review required."
                        : "Delivery saved. Job completed.";

                    await Shell.Current.DisplayAlertAsync("Saved", msg, "OK");

                    await Shell.Current.GoToAsync($"//{nameof(DashboardPage)}");
                },
                _crashLogger,
                "DeliverySignatureViewModel.SaveAsync",
                nameof(DeliverySignaturePage),
                metadataJson: $"{{\"JobId\":\"{_job.Id}\",\"ReceiverName\":\"{ReceiverName}\",\"HasWaitTime\":{WaitTimeMinutes.HasValue.ToString().ToLowerInvariant()},\"HasDamageNotes\":{(!string.IsNullOrWhiteSpace(DamageNotes)).ToString().ToLowerInvariant()}}}",
                onError: async ex =>
                {
                    await Shell.Current.DisplayAlertAsync("Save failed", ex.Message, "OK");
                });
        }
        finally
        {
            _isSaving = false;
            ((Command)SaveCommand).ChangeCanExecute();
        }
    }

    private bool HasSignature() => _strokes.Any(s => s.Count > 2);

    private string BuildSignatureJson()
    {
        var data = new SignatureData(
            _strokes.Select(stroke =>
                new SignatureStroke(
                    stroke.Select(p => new SigPoint(p.X, p.Y)).ToList()
                )
            ).ToList()
        );

        return JsonSerializer.Serialize(data);
    }

    #endregion
}

#region Events

public record JobCompletedEvent(Guid JobId);

public interface IAppEventBus
{
    Task PublishAsync<T>(T message);
    void Subscribe<T>(object subscriber, Action<T> handler);
    void Unsubscribe(object subscriber);
}

#endregion

#region Signature JSON Models

public record SigPoint(float X, float Y);
public record SignatureStroke(List<SigPoint> Points);
public record SignatureData(List<SignatureStroke> Strokes);

#endregion

#region Drawable

public class SignatureDrawable : IDrawable
{
    #region Dependencies

    private readonly List<List<PointF>> _strokes;

    #endregion

    #region Events

    public Action? InvalidateRequested;

    #endregion

    #region Constructor

    public SignatureDrawable(List<List<PointF>> strokes)
    {
        _strokes = strokes;
    }

    #endregion

    #region Methods

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FillColor = Colors.Transparent;
        canvas.StrokeColor = Colors.Black;
        canvas.StrokeSize = 3;

        foreach (var stroke in _strokes)
        {
            for (int i = 1; i < stroke.Count; i++)
                canvas.DrawLine(stroke[i - 1], stroke[i]);
        }
    }

    #endregion
}

#endregion