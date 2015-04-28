using GalaSoft.MvvmLight.Ioc;
using Mirko_v2.Utils;
using Mirko_v2.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
    public sealed partial class Entry : UserControl
    {
        private bool singleTap;

        public WykopAPI.Models.Entry EntryData
        {
            get { return (WykopAPI.Models.Entry)GetValue(EntryDataProperty); }
            set { SetValue(EntryDataProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EntryData.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EntryDataProperty =
            DependencyProperty.Register("EntryData", typeof(WykopAPI.Models.Entry), typeof(Entry), new PropertyMetadata(null, EntryDataChanged));

        private static void EntryDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            throw new NotImplementedException();
        }        

        public bool ShowComments
        {
            get { return (bool)GetValue(ShowCommentsProperty); }
            set { SetValue(ShowCommentsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowComments.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowCommentsProperty =
            DependencyProperty.Register("ShowComments", typeof(bool), typeof(Entry), new PropertyMetadata(false));

        #region IsHot
        public bool IsHot
        {
            get { return (bool)GetValue(IsHotProperty); }
            set { SetValue(IsHotProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsHot.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsHotProperty =
            DependencyProperty.Register("IsHot", typeof(bool), typeof(Entry), new PropertyMetadata(false));
        #endregion

        #region Events
        public delegate void PageNavigationEventHandler(object sender, PageNavigationEventArgs e);
        public event PageNavigationEventHandler NavigateTo;

        private void Entry_NavigateTo(object sender, PageNavigationEventArgs e)
        {
            if (NavigateTo != null)
                NavigateTo(sender, e);
        }

        private void Embed_NavigateTo(object sender, PageNavigationEventArgs e)
        {
            if (NavigateTo != null)
                NavigateTo(sender, e);
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
                    (DataContext as EntryViewModel).GoToEntryPage();
            }
        }

        private void UserControl_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (e != null)
                e.Handled = true;

            singleTap = false;

            if (DataContext != null)
                (DataContext as EntryViewModel).VoteCommand.Execute(null);
        }
    }
}
