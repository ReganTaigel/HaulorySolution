using System.ComponentModel;
using System.Runtime.CompilerServices;
using Haulory.Contracts.Vehicles;

namespace Haulory.Mobile.ViewModels;

public class VehicleListItemViewModel : INotifyPropertyChanged
{
    private bool _isExpanded;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public Guid Id { get; set; }

    public int Year { get; set; }
    public string Make { get; set; } = "";
    public string Model { get; set; } = "";

    public string VehicleTypeDisplay { get; set; } = "";
    public string Rego { get; set; } = "";

    public string RegoExpiryDisplay { get; set; } = "";
    public string RegoStatusInlineDisplay { get; set; } = "";

    public string CertificateNameDisplay { get; set; } = "";
    public string CertificateExpiryDisplay { get; set; } = "";
    public string CertStatusInlineDisplay { get; set; } = "";

    public string OdometerFullDisplay { get; set; } = "";

    public bool IsRucApplicable { get; set; }

    public string RucLicenceStartDisplay { get; set; } = "";
    public string RucLicenceEndDisplay { get; set; } = "";
    public string RucRemainingDisplay { get; set; } = "";

    public string IsRucOverdueYesNo { get; set; } = "";

    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (_isExpanded == value)
                return;

            _isExpanded = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ExpandIcon));
            OnPropertyChanged(nameof(ExpandText));
        }
    }

    public string ExpandIcon => IsExpanded ? "▲" : "▼";
    public string ExpandText => IsExpanded ? "Hide details" : "Show details";

    public static VehicleListItemViewModel FromDto(VehicleDto dto)
    {
        var model = new VehicleListItemViewModel
        {
            Id = dto.Id,
            Year = dto.Year,
            Make = dto.Make,
            Model = dto.Model,
            Rego = dto.Rego,
            VehicleTypeDisplay = dto.VehicleType ?? dto.Kind ?? "Vehicle",
            OdometerFullDisplay = dto.OdometerKm.HasValue
                ? $"{dto.OdometerKm.Value:N0} km"
                : "—"
        };

        if (dto.RegoExpiry != null)
        {
            model.RegoExpiryDisplay = dto.RegoExpiry.Value.ToString("dd MMM yyyy");
            model.RegoStatusInlineDisplay = dto.RegoExpiry < DateTime.UtcNow ? "Expired" : "Valid";
        }
        else
        {
            model.RegoExpiryDisplay = "—";
        }

        model.CertificateNameDisplay = dto.CertificateType ?? "Certificate";

        if (dto.CertificateExpiry != null)
        {
            model.CertificateExpiryDisplay = dto.CertificateExpiry.Value.ToString("dd MMM yyyy");
            model.CertStatusInlineDisplay = dto.CertificateExpiry < DateTime.UtcNow ? "Expired" : "Valid";
        }
        else
        {
            model.CertificateExpiryDisplay = "—";
        }

        if (dto.RucLicenceStartKm != null && dto.RucLicenceEndKm != null)
        {
            model.IsRucApplicable = true;

            model.RucLicenceStartDisplay = dto.RucLicenceStartKm.Value.ToString("N0");
            model.RucLicenceEndDisplay = dto.RucLicenceEndKm.Value.ToString("N0");

            if (dto.OdometerKm.HasValue)
            {
                var remaining = dto.RucLicenceEndKm.Value - dto.OdometerKm.Value;
                model.RucRemainingDisplay = $"{remaining:N0} km";
                model.IsRucOverdueYesNo = remaining < 0 ? "Yes" : "No";
            }
            else
            {
                model.RucRemainingDisplay = "—";
                model.IsRucOverdueYesNo = "No";
            }
        }

        return model;
    }
}