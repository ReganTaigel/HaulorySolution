using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Haulory.Mobile.ViewModels;

public class JobGroupViewModel : BaseViewModel
{
    private bool _isExpanded = true;

    public string Title { get; }
    public bool IsMainGroup { get; }

    public ObservableCollection<JobListItemViewModel> Jobs { get; } = new();

    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (_isExpanded == value) return;
            _isExpanded = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ExpandCollapseGlyph));
        }
    }

    public string ExpandCollapseGlyph => IsExpanded ? "▲" : "▼";

    public ICommand ToggleExpandedCommand { get; }

    public JobGroupViewModel(string title, bool isMainGroup = false)
    {
        Title = title;
        IsMainGroup = isMainGroup;

        ToggleExpandedCommand = new Command(() =>
        {
            IsExpanded = !IsExpanded;
        });
    }
}