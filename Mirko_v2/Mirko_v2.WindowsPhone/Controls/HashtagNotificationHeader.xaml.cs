using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko_v2.Controls
{
    public sealed partial class HashtagNotificationHeader : UserControl
    {
        public HashtagNotificationHeader()
        {
            this.InitializeComponent();
        }

        public void Hide()
        {
            var anim = this.Resources["HideAnimation"] as Storyboard;
            anim.Begin();
        }

        public void Show()
        {
            var anim = this.Resources["ShowAnimation"] as Storyboard;
            anim.Begin();
        }
    }
}
