using Mirko_v2.Utils;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko_v2.Controls
{
    public sealed partial class LennyChooser : UserControl
    {
        public LennyChooser()
        {
            this.InitializeComponent();
        }

        public delegate void LennySelectedEventHandler(object sender, StringEventArgs e);
        public event LennySelectedEventHandler LennySelected;

        private void Lenny_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var tb = sender as TextBlock;
            if (tb == null) return;

            if (this.LennySelected != null)
                this.LennySelected(this, new StringEventArgs(tb.Text));
        }
    }
}
