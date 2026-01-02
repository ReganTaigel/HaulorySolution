using Haulory.Moblie.Views;
using Microsoft.Maui.Controls;

namespace Haulory.Moblie;

public partial class App : Microsoft.Maui.Controls.Application
{
    public App(AppShell shell)
    {
        InitializeComponent();
        MainPage = shell;
    }
}
