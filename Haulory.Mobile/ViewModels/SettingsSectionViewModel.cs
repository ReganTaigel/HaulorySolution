using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Haulory.Mobile.ViewModels;

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
        }
    }

    public string ExpandCollapseGlyph => IsExpanded ? "▲" : "▼";

    public ICommand ToggleExpandedCommand { get; }

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