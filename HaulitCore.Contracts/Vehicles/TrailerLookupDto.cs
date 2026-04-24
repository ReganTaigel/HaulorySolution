namespace HaulitCore.Contracts.Vehicles
{
    public class TrailerLookupDto
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Rego { get; set; } = string.Empty;
        public int? HubodometerKm { get; set; }
    }
}
