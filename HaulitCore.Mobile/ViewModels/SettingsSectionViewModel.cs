using HaulitCore.Mobile.ViewModels;
using System.Windows.Input;

public class SettingsSectionViewModel : BaseViewModel
{
    private bool _isExpanded;

    public string Title { get; }

    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (_isExpanded == value)
                return;

            _isExpanded = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ExpandCollapseGlyph));

            ExpandedChanged?.Invoke(this, _isExpanded);
        }
    }

    public string ExpandCollapseGlyph => IsExpanded ? "▲" : "▼";

    public ICommand ToggleExpandedCommand { get; }

    public event Action<SettingsSectionViewModel, bool>? ExpandedChanged;

    public SettingsSectionViewModel(string title, bool isExpanded = false)
    {
        Title = title;
        _isExpanded = isExpanded;

        ToggleExpandedCommand = new Command(() =>
        {
            IsExpanded = !IsExpanded;
        });
    }
}