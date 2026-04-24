namespace HaulitCore.Mobile.Models;

public class TrailerOption
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public int? HubodometerKm { get; set; }
    public bool IsSelected { get; set; }
}