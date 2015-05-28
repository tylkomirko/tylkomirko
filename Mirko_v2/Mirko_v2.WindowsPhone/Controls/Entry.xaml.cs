using Mirko_v2.ViewModel;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko_v2.Controls
{
    public sealed partial class Entry : UserControl
    {
        private bool singleTap;

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
                embed.MaxHeight = 450;
                embed.MaxWidth = 450;
            }
            else
            {
                embed.MaxHeight = 250;
                embed.MaxWidth = 250;
            }
        }

        public bool ShowComments
        {
            get { return (bool)GetValue(ShowCommentsProperty); }
            set { SetValue(ShowCommentsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowComments.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowCommentsProperty =
            DependencyProperty.Register("ShowComments", typeof(bool), typeof(Entry), new PropertyMetadata(false));

        public bool ShowLeftSpacer
        {
            get { return (bool)GetValue(ShowLeftSpacerProperty); }
            set { SetValue(ShowLeftSpacerProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowLeftSpacer.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowLeftSpacerProperty =
            DependencyProperty.Register("ShowLeftSpacer", typeof(bool), typeof(Entry), new PropertyMetadata(false, ShowLeftSpacerChanged));

        private static void ShowLeftSpacerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
                (d as Entry).LeftSpacer.Width = 30.0;
        }
        #endregion

        public Entry()
        {
            this.InitializeComponent();            
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

        private void UserControl_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (e != null)
                e.Handled = true;

            singleTap = false;

            if (DataContext != null)
            {
                var entryVM = DataContext as EntryViewModel;
                if(entryVM != null)
                {
                    entryVM.VoteCommand.Execute(null);
                }
                else
                {
                    var commentVM = DataContext as CommentViewModel;
                    if (commentVM != null)
                        commentVM.VoteCommand.Execute(null);

                }
            }
        }

        public void HashtagTapped(string tag, TextBlock tb)
        {
            if (string.IsNullOrEmpty(tag) || tb == null) return;

            var mf = Resources["HashtagFlyout"] as MenuFlyout;
            var VM = DataContext as EntryViewModel;
            VM.PrepareHashtagFlyout(ref mf, tag);            

            mf.ShowAt(tb);
        }

        private void VoteTB_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (e != null)
                e.Handled = true;

            if (DataContext != null)
            {
                var entryVM = DataContext as EntryViewModel;
                if (entryVM != null)
                {
                    entryVM.VoteCommand.Execute(null);
                }
                else
                {
                    var commentVM = DataContext as CommentViewModel;
                    if (commentVM != null)
                        commentVM.VoteCommand.Execute(null);

                }
            }
        }
    }
}
