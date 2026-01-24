using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MagoLauncher.Presentation.Views;

public partial class ModpackDetailView : UserControl
{
    public ModpackDetailView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
