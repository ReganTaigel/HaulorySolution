using Haulory.Mobile.Contracts.Vehicles;

namespace Haulory.Mobile.ViewModels;

public class VehicleListItemViewModel
{
    public Guid Id { get; set; }

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

    public static VehicleListItemViewModel FromDto(VehicleDto dto)
    {
        var model = new VehicleListItemViewModel
        {
            Id = dto.Id,
            Rego = dto.Rego,
            VehicleTypeDisplay = dto.VehicleType ?? dto.Kind ?? "Vehicle",

            OdometerFullDisplay = dto.OdometerKm.HasValue
                ? $"{dto.OdometerKm.Value:N0} km"
                : "—"
        };

        if (dto.RegoExpiry != null)
        {
            model.RegoExpiryDisplay = dto.RegoExpiry.Value.ToString("dd MMM yyyy");

            if (dto.RegoExpiry < DateTime.UtcNow)
                model.RegoStatusInlineDisplay = "Expired";
            else
                model.RegoStatusInlineDisplay = "Valid";
        }

        if (dto.CertificateExpiry != null)
        {
            model.CertificateNameDisplay = dto.CertificateType ?? "Certificate";
            model.CertificateExpiryDisplay = dto.CertificateExpiry.Value.ToString("dd MMM yyyy");

            if (dto.CertificateExpiry < DateTime.UtcNow)
                model.CertStatusInlineDisplay = "Expired";
            else
                model.CertStatusInlineDisplay = "Valid";
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
        }

        return model;
    }
}