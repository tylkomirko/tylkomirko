using Windows.UI.Xaml.Controls;

namespace Mirko.Controls
{
    public static class CommandBarExtensions
    {
        public static void Hide(this CommandBar appBar)
        {
            if (appBar.ClosedDisplayMode == AppBarClosedDisplayMode.Compact)
                appBar.ClosedDisplayMode = AppBarClosedDisplayMode.Minimal;
        }

        public static void Show(this CommandBar appBar)
        {
            if (appBar.ClosedDisplayMode == AppBarClosedDisplayMode.Minimal)
                appBar.ClosedDisplayMode = AppBarClosedDisplayMode.Compact;
        }
    }
}
