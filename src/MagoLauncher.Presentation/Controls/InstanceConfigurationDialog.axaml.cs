using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MagoLauncher.Presentation.Controls;

public partial class InstanceConfigurationDialog : UserControl
{
    public InstanceConfigurationDialog()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
