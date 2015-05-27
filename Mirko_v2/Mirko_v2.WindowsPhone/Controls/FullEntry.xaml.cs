using GalaSoft.MvvmLight.Ioc;
using Mirko_v2.Utils;
using Mirko_v2.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko_v2.Controls
{
    public class BlacklistBlockTemplateSelector : DataTemplateSelector
    {
        public DataTemplate EntryTemplate { get; set; }
        public DataTemplate HiddenTemplate { get; set; }
        public DataTemplate NullTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            var block = item as BlacklistHelper.Block;

            if (block.Type == BlacklistHelper.BlockType.ENTRY)
                return EntryTemplate;
            else if (block.Type == BlacklistHelper.BlockType.HIDDEN_ENTRIES)
                return HiddenTemplate;
            else
                return NullTemplate;
        }
    }

    public sealed partial class FullEntry : UserControl
    {
        public FullEntry()
        {
            this.InitializeComponent();
            
            this.DataContextChanged += UserControl_DataContextChanged;
        }

        #region Fullscreen
        public bool Fullscreen
        {
            get { return (bool)GetValue(FullscreenProperty); }
            set { SetValue(FullscreenProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Fullscreen.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FullscreenProperty =
            DependencyProperty.Register("Fullscreen", typeof(bool), typeof(FullEntry), new PropertyMetadata(false));
        #endregion

        #region IsHot
        public bool IsHot
        {
            get { return (bool)GetValue(IsHotProperty); }
            set { SetValue(IsHotProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsHot.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsHotProperty =
            DependencyProperty.Register("IsHot", typeof(bool), typeof(FullEntry), new PropertyMetadata(false));
        #endregion

        #region Blacklist
        private ObservableCollectionEx<BlacklistHelper.Block> _blacklistBlocks = null;
        public ObservableCollectionEx<BlacklistHelper.Block> BlacklistBlocks
        {
            get { return _blacklistBlocks ?? (_blacklistBlocks = new ObservableCollectionEx<BlacklistHelper.Block>()); }
        }
        #endregion

        private void UserControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            var entry = args.NewValue as EntryViewModel;
            if (entry == null) return;

            if (Fullscreen)
            {
                // process blacklisted comments
                BlacklistBlocks.Clear();
                BlacklistBlocks.AddRange(BlacklistHelper.ProcessComments(entry.Comments));

                CommentCountBorder.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                //Footer.Visibility = Windows.UI.Xaml.Visibility.Visible;
                //HeaderSpacer.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            else
            {
                ListView.ItemsSource = null;
                if (entry.Data.CommentCount > 0)
                    CommentCountBorder.Visibility = Windows.UI.Xaml.Visibility.Visible;
                else
                    CommentCountBorder.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                ListView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                //Footer.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                //HeaderSpacer.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
        }

        private void HiddenBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var hiddenBlockGrid = sender as StackPanel;
            var hiddenBlock = hiddenBlockGrid.DataContext as BlacklistHelper.Block;

            foreach (var id in hiddenBlock.EntriesIDs)
            {
                for (int idx = 0; idx < BlacklistBlocks.Count; idx++)
                {
                    var currentBlock = BlacklistBlocks[idx];
                    if (currentBlock.Comment.Data.ID == id)
                    {
                        var updatedBlock = currentBlock;
                        updatedBlock.Type = BlacklistHelper.BlockType.ENTRY;
                        BlacklistBlocks.Replace(idx, updatedBlock);

                        break;
                    }
                }
            }
        }

        private void CommentCountBorder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var entry = this.DataContext as EntryViewModel;
            if (entry != null)
            {
                ListView.Visibility = Visibility.Visible;
                CommentCountBorder.Visibility = Visibility.Collapsed;

                ListView.SetBinding(ListView.ItemsSourceProperty, new Windows.UI.Xaml.Data.Binding() { Source = BlacklistBlocks });
                BlacklistBlocks.Clear();

                if(IsHot && entry.Comments.Count == 0)
                {
                    entry.Comments.CollectionChanged += Comments_CollectionChanged;
                    entry.GetComments.Execute(null);
                }
                else
                {
                    BlacklistBlocks.AddRange(BlacklistHelper.ProcessComments(entry.Comments));
                }
            }
        }

        private void Comments_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if(e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                var comments = sender as ObservableCollectionEx<CommentViewModel>;
                BlacklistBlocks.AddRange(BlacklistHelper.ProcessComments(comments));

                comments.CollectionChanged -= Comments_CollectionChanged;
            }
        }
    }
}
