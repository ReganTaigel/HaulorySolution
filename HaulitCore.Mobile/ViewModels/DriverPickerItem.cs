namespace HaulitCore.Mobile.ViewModels;

public class DriverPickerItem
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
}