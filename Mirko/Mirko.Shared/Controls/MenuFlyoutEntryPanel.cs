using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Mirko.Controls
{
    public class MenuFlyoutEntryPanel : MenuFlyoutItem
    {
        private AppBarButton EditButton { get; set; }
        private AppBarButton DeleteButton { get; set; }
        private AppBarToggleButton FavButton { get; set; }

        public bool EnableEditButton
        {
            get { return (bool)GetValue(EnableEditButtonProperty); }
            set { SetValue(EnableEditButtonProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EnableEditButton.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnableEditButtonProperty =
            DependencyProperty.Register("EnableEditButton", typeof(bool), typeof(MenuFlyoutEntryPanel), new PropertyMetadata(false, EnableEditButtonChanged));

        private static void EnableEditButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var newValue = (bool)e.NewValue;
            var panel = d as MenuFlyoutEntryPanel;
            if (panel.EditButton != null)
                panel.EditButton.IsEnabled = newValue;
        }

        public bool EnableDeleteButton
        {
            get { return (bool)GetValue(EnableDeleteButtonProperty); }
            set { SetValue(EnableDeleteButtonProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EnableDeleteButton.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnableDeleteButtonProperty =
            DependencyProperty.Register("EnableDeleteButton", typeof(bool), typeof(MenuFlyoutEntryPanel), new PropertyMetadata(false));

        public bool EnableFavButton
        {
            get { return (bool)GetValue(EnableFavButtonProperty); }
            set { SetValue(EnableFavButtonProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EnableFavButton.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnableFavButtonProperty =
            DependencyProperty.Register("EnableFavButton", typeof(bool), typeof(MenuFlyoutEntryPanel), new PropertyMetadata(true));       

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            EditButton = (AppBarButton)GetTemplateChild("editButton");
            EditButton.IsEnabled = EnableEditButton;

            DeleteButton = (AppBarButton)GetTemplateChild("deleteButton");
            DeleteButton.IsEnabled = EnableDeleteButton;

            FavButton = (AppBarToggleButton)GetTemplateChild("favButton");
            FavButton.IsEnabled = EnableFavButton;
        }
    }
}
