using GalaSoft.MvvmLight.Ioc;
using Mirko.Controls;
using Mirko.Utils;
using Mirko.ViewModel;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko.Pages
{
    public class HashtagSelectionPageTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ItemTemplate { get; set; }
        public DataTemplate ItemTemplateNoCount { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            var VM = SimpleIoc.Default.GetInstance<NotificationsViewModel>();

            if (VM.HashtagNotificationsCount > 0)
                return ItemTemplate;
            else
                return ItemTemplateNoCount;
        }
    }

    public sealed partial class HashtagSelectionPage : UserControl, IHaveAppBar
    {
        public HashtagSelectionPage()
        {
            this.InitializeComponent();
        }

        private void ListView_ScrollingDown(object sender, EventArgs e)
        {
            AppBar.Hide();
        }

        private void ListView_ScrollingUp(object sender, EventArgs e)
        {
            AppBar.Show();
        }

        private void HashtagSuggestionBox_HashtagSelected(object sender, StringEventArgs e)
        {
            var tag = e.String;
            var flyout = Resources["HashtagFlyout"] as FlyoutBase;
            flyout.Hide();

            SimpleIoc.Default.GetInstance<MainViewModel>().GoToHashtagPage.Execute(tag);
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var newItem = e.ClickedItem as HashtagInfoContainer;
            if (newItem == null) return;

            var VM = SimpleIoc.Default.GetInstance<NotificationsViewModel>();
            VM.CurrentHashtag = newItem;
            VM.GoToHashtagNotificationsPage.Execute(null);
        }

        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            var height = SimpleIoc.Default.GetInstance<MainViewModel>().ListViewHeaderHeight;
            ListView.Margin = new Thickness(10, -height, 0, 0);

            var sp = ListView.Header as StackPanel;
            var rect = sp.GetDescendant<Rectangle>();
            rect.Height = height;
        }

        #region AppBar
        private CommandBar AppBar = null;

        public CommandBar CreateCommandBar()
        {
            var c = new CommandBar();

            var find = new AppBarButton()
            {
                Icon = new SymbolIcon(Symbol.Find),
                Label = "szukaj",
            };
            find.Click += FindHashtag_Click;

            var removeAll = new AppBarButton()
            {
                Label = "usuń wszystkie powiadomienia"
            };
            removeAll.SetBinding(AppBarButton.CommandProperty, new Binding()
            {
                Source = this.DataContext as NotificationsViewModel,
                Path = new PropertyPath("DeleteAllHashtagNotifications"),
            });

            c.PrimaryCommands.Add(find);
            c.SecondaryCommands.Add(removeAll);
            AppBar = c;

            return c;
        }

        private void FindHashtag_Click(object sender, RoutedEventArgs e)
        {
            var flyout = Resources["HashtagFlyout"] as FlyoutBase;
            flyout.ShowAt(this);
        }
        #endregion
    }
}
