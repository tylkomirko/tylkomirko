using Mirko.Utils;
using Mirko.ViewModel;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using System;
using GalaSoft.MvvmLight.Ioc;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko.Controls
{
    public sealed partial class Entry : UserControl, IReceiveRTBClicks
    {
        private bool singleTap;

        private EntryBaseViewModel VM
        {
            get { return DataContext as EntryBaseViewModel; }
        }

        #region Registered properties
        public bool IsHot
        {
            get { return (bool)GetValue(IsHotProperty); }
            set { SetValue(IsHotProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsHot.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsHotProperty =
            DependencyProperty.Register("IsHot", typeof(bool), typeof(Entry), new PropertyMetadata(false));

        public bool LargeEmbed
        {
            get { return (bool)GetValue(LargeEmbedProperty); }
            set { SetValue(LargeEmbedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LargeEmbed.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LargeEmbedProperty =
            DependencyProperty.Register("LargeEmbed", typeof(bool), typeof(Entry), new PropertyMetadata(false, LargeEmbedChanged));

        private static void LargeEmbedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var entryControl = d as Controls.Entry;
            var embed = entryControl.EmbedPreview;
            var isLarge = (bool)e.NewValue;
            if(isLarge)
            {
                embed.MaxHeight = 400;
                embed.MaxWidth = 400;
            }
            else
            {
                embed.MaxHeight = 220;
                embed.MaxWidth = 220;
            }
        }

        public bool EnableTextSelection
        {
            get { return (bool)GetValue(EnableTextSelectionProperty); }
            set { SetValue(EnableTextSelectionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EnableTextSelection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnableTextSelectionProperty =
            DependencyProperty.Register("EnableTextSelection", typeof(bool), typeof(Entry), new PropertyMetadata(false));      
        #endregion

        public Entry()
        {
            this.InitializeComponent();
            this.Holding += Entry_OpenFlyout;
            this.RightTapped += Entry_OpenFlyout;

            var settingsVM = SimpleIoc.Default.GetInstance<SettingsViewModel>();
            GoToVisualState(settingsVM.ShowAvatars);
            settingsVM.PropertyChanged += (s, args) =>
            {
                if (args.PropertyName == "ShowAvatars")
                    GoToVisualState(settingsVM.ShowAvatars);
            };
        }

        private void GoToVisualState(bool showAvatars)
        {
            if (showAvatars)
                VisualStateManager.GoToState(this, ShowAvatars.Name, false);
            else
                VisualStateManager.GoToState(this, Regular.Name, false);
        }

        private void Entry_OpenFlyout(object sender, RoutedEventArgs e)
        {
            var holding = e as HoldingRoutedEventArgs;
            var rightTap = e as RightTappedRoutedEventArgs;

            if (holding == null && rightTap == null) return;

            if (holding != null &&
                (holding.HoldingState == Windows.UI.Input.HoldingState.Completed ||
                 holding.HoldingState == Windows.UI.Input.HoldingState.Canceled)) return;

            var pos = holding != null ? holding.GetPosition(EmbedPreviewGrid) : rightTap.GetPosition(EmbedPreviewGrid);

            System.Diagnostics.Debug.WriteLine("pos: " + pos);

            if (VM.EmbedVM != null && pos.Y > 0 && pos.X < EmbedPreview.ActualWidth)
            {
                var mf = FlyoutBase.GetAttachedFlyout(EmbedPreview);
                mf.ShowAt(EmbedPreviewGrid);
            }
            else
            {
                var helper = new OpenEntryMenuFlyout();
                helper.Execute(this, null);
            }

            if (holding != null)
                holding.Handled = true;
            else
                rightTap.Handled = true;
        }

        private async void UserControl_Tapped(object sender, TappedRoutedEventArgs e)
        {
            singleTap = true;
            await Task.Delay(100); // ugly trick, I know

            if (this.IsTapEnabled && singleTap)
            {
                if (DataContext != null) 
                {
                    (DataContext as EntryViewModel).GoToEntryPage(IsHot);
                    e.Handled = true;
                }
            }
        }

        private void VoteTB_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (e != null)
                e.Handled = true;

            if (DataContext != null)
            {
                var entryVM = DataContext as EntryBaseViewModel;
                if (entryVM != null)
                    entryVM.VoteCommand.Execute(null);
            }
        }

        private void UserControl_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (e != null)
                e.Handled = true;

            singleTap = false;

            if (DataContext != null)
            {
                var entryVM = DataContext as EntryBaseViewModel;
                if(entryVM != null)
                    entryVM.VoteCommand.Execute(null);
            }
        }

        private void FavouriteButton_Click(object sender, RoutedEventArgs e)
        {
            var mf = FlyoutBase.GetAttachedFlyout(EntryGrid) as MenuFlyout;
            mf.Hide();
        }

        private void AuthorTB_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (e != null)
                e.Handled = true;

            var tb = sender as TextBlock;
            if (tb == null) return;

            var username = tb.Text;
            ProfileTapped(username);
        }

        public void HashtagTapped(string tag, TextBlock tb)
        {
            if (string.IsNullOrEmpty(tag) || tb == null) return;

            var mf = Resources["HashtagFlyout"] as MenuFlyout;
            InjectedRTBHelper.PrepareHashtagFlyout(ref mf, tag);
            VM.TappedHashtag = tag;          

            mf.ShowAt(tb);
        }

        public void ProfileTapped(string username)
        {
            if (string.IsNullOrEmpty(username)) return;
            InjectedRTBHelper.GoToProfilePage(username);
        }

        private void MenuFlyoutItem_ShowVoters_Click(object sender, RoutedEventArgs e)
        {
            if (VM.DataBase.VoteCount != VM.DataBase.Voters.Count)
                VM.RefreshCommand.Execute(null);

            VM.ShowVoters = true;
            VotersRTB.Visibility = Visibility.Visible;
        }

        public delegate void TextSelectionChangedEventHandler(object sender, StringEventArgs e);
        public event TextSelectionChangedEventHandler TextSelectionChanged;

        private void BodyRTB_SelectionChanged(object sender, RoutedEventArgs e)
        {
            var txt = BodyRTB.SelectedText;

            if (TextSelectionChanged != null)
                TextSelectionChanged(this, new StringEventArgs(txt));
        }
    }
}
