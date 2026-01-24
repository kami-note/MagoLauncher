using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MagoLauncher.Presentation.Views
{
    public partial class StoreView : UserControl
    {
        public StoreView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
