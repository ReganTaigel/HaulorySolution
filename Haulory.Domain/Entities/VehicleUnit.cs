namespace Haulory.Domain.Entities;

public class VehicleUnit
{
    public int UnitNumber { get; set; } // 1,2,3

    public int Year { get; set; }  

    public string Rego { get; set; } = string.Empty;
    public DateTime? RegoExpiry { get; set; }

    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;

    public DateTime? CertificateExpiry { get; set; }
    public int? OdometerKm { get; set; }

}
