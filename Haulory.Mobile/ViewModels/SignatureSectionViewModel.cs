using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Haulory.Mobile.ViewModels;

public class SignatureSectionViewModel : INotifyPropertyChanged
{
    private bool _isExpanded = true;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

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

    // Simpler characters render more reliably on Android
    public string ExpandIcon => IsExpanded ? "−" : "+";

    public string ExpandText => IsExpanded ? "Hide signature" : "Show signature";

    public ICommand ToggleExpandedCommand { get; }

    public SignatureSectionViewModel()
    {
        ToggleExpandedCommand = new Command(ToggleExpanded);
    }

    private void ToggleExpanded()
    {
        IsExpanded = !IsExpanded;
    }
}