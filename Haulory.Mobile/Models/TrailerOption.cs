namespace Haulory.Mobile.Models;

public class TrailerOption
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public int? OdometerKm { get; set; }
    public bool IsSelected { get; set; }
}