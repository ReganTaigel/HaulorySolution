using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;
using Haulory.Infrastructure.Persistence.Json;
using Microsoft.Maui.Graphics;
using System.Text.Json;
using System.Windows.Input;

namespace Haulory.Moblie.ViewModels;

[QueryProperty(nameof(JobId), "jobId")]
public class DeliverySignatureViewModel : BaseViewModel
{
    #region Fields

    private readonly IJobRepository _jobRepository;
    private readonly IDeliveryReceiptRepository _deliveryReceiptRepository;

    private Job? _job;
    private bool _isSaving;

    private string _receiverName = string.Empty;
    private string _statusMessage = string.Empty;

    private Guid _jobId;

    #endregion

    #region Bindable Properties (Job Details)

    public string ReferenceNumber => _job?.ReferenceNumber ?? string.Empty;
    public string PickupCompany => _job?.PickupCompany ?? string.Empty;
    public string DeliveryCompany => _job?.DeliveryCompany ?? string.Empty;
    public string DeliveryAddress => _job?.DeliveryAddress ?? string.Empty;
    public string LoadDescription => _job?.LoadDescription ?? string.Empty;

    public bool IsDelivered => _job?.IsDelivered ?? false;

    #endregion

    #region Receiver

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

    #endregion

    #region Status

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

    #endregion

    #region Signature Data (Strokes)

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
            if (Guid.TryParse(value, out var id))
            {
                _jobId = id;
                _ = LoadJobAsync();
            }
        }
    }

    #endregion

    #region Constructor

    public DeliverySignatureViewModel(IJobRepository jobRepository,
    IDeliveryReceiptRepository deliveryReceiptRepository)
    {
        _jobRepository = jobRepository;
        _deliveryReceiptRepository = deliveryReceiptRepository;

        SignatureDrawable = new SignatureDrawable(_strokes);

        ClearSignatureCommand = new Command(() =>
        {
            _strokes.Clear();
            _currentStroke = null;

            SignatureDrawable.InvalidateRequested?.Invoke();
            StatusMessage = "Signature cleared.";
        });

        SaveCommand = new Command(async () => await SaveAsync());
    }

    #endregion

    #region Load

    private async Task LoadJobAsync()
    {
        _job = await _jobRepository.GetByIdAsync(_jobId);

        if (_job == null)
        {
            StatusMessage = "Job not found.";
            return;
        }

        OnPropertyChanged(nameof(ReferenceNumber));
        OnPropertyChanged(nameof(PickupCompany));
        OnPropertyChanged(nameof(DeliveryCompany));
        OnPropertyChanged(nameof(DeliveryAddress));
        OnPropertyChanged(nameof(LoadDescription));
        OnPropertyChanged(nameof(IsDelivered));

        StatusMessage = _job.IsDelivered
            ? "This job has already been delivered."
            : "Sign and save delivery.";
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

    #region Save


    private async Task SaveAsync()
    {
        if (_isSaving) return;
        _isSaving = true;

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

            // Build signature JSON (strokes)
            var signatureJson = BuildSignatureJson();

            // Create receipt snapshot for accounting/audit
            var receipt = new DeliveryReceipt(
                jobId: _job.Id,
                referenceNumber: _job.ReferenceNumber,
                invoiceNumber: _job.InvoiceNumber,
                pickupCompany: _job.PickupCompany,
                pickupAddress: _job.PickupAddress,
                deliveryCompany: _job.DeliveryCompany,
                deliveryAddress: _job.DeliveryAddress,
                loadDescription: _job.LoadDescription,
                rateType: _job.RateType,
                rateValue: _job.RateValue,
                quantity: _job.Quantity,
                total: _job.Total,
                receiverName: ReceiverName.Trim(),
                deliveredAtUtc: DateTime.UtcNow,
                signatureJson: signatureJson
            );

            // 1) Save receipt to delivery_receipts.json
            await _deliveryReceiptRepository.AddAsync(receipt);

            // 2) Remove job from jobs.json so it disappears from active list
            await _jobRepository.DeleteAsync(_job.Id);

            await Shell.Current.DisplayAlertAsync("Saved", "Delivery signed and saved.", "OK");
            await Shell.Current.GoToAsync("..");
        }
        finally
        {
            _isSaving = false;
        }
    }


    private bool HasSignature()
        => _strokes.Any(s => s.Count > 2);

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

#region Signature JSON Models

public record SigPoint(float X, float Y);
public record SignatureStroke(List<SigPoint> Points);
public record SignatureData(List<SignatureStroke> Strokes);

#endregion

#region Drawable

public class SignatureDrawable : IDrawable
{
    private readonly List<List<PointF>> _strokes;
    public Action? InvalidateRequested;

    public SignatureDrawable(List<List<PointF>> strokes)
    {
        _strokes = strokes;
    }

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
}

#endregion
