using Mirko_v2.Utils;
using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Mirko_v2.Controls
{
    public class ListViewEx : ListView
    {
        #region AppBar
        public CommandBar AppBar
        {
            get { return (CommandBar)GetValue(AppBarProperty); }
            set { SetValue(AppBarProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AppBar.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AppBarProperty =
            DependencyProperty.Register("AppBar", typeof(CommandBar), typeof(ListViewEx), new PropertyMetadata(null));
        #endregion

        private ScrollViewer ScrollViewer = null;

        public ListViewEx()
        {
            base.Loaded += ListViewEx_Loaded;
            //base.Unloaded += ListViewEx_Unloaded;
        }

        private void ListViewEx_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (ScrollViewer == null)
                ScrollViewer = this.GetDescendant<ScrollViewer>();

            ScrollViewer.ViewChanged += ScrollViewer_ViewChanged;
        }

        private void ListViewEx_Unloaded(object sender, Windows.UI.Xaml.RoutedEventArgs e) // is this necessary?
        {
            if (ScrollViewer != null)
                ScrollViewer.ViewChanged -= ScrollViewer_ViewChanged;
        }

        #region Scrolling detection
        private const int OffsetDelta = 30;
        private const int CounterTrigger = 10;

        private bool ScrollingDownCalled = false;
        private bool ScrollingUpCalled = false;

        private int ScrollingUpCounter = 0;
        private int ScrollingDownCounter = 0;

        private double PreviousOffset = 0;
        private double Offset = 0;

        public event EventHandler ScrollingDown;
        public delegate void ScrollingDownEventHandler(object sender, EventArgs args);
        public event EventHandler ScrollingUp;
        public delegate void ScrollingUpEventHandler(object sender, EventArgs args);

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var sV = sender as ScrollViewer;
            //var newOffset = Convert.ToInt32(sV.VerticalOffset) / 2;
            var newOffset = sV.VerticalOffset;

            PreviousOffset = Offset;
            Offset = newOffset;

            var delta = Offset - PreviousOffset;
            //Debug.WriteLine("delta: " + delta);

            bool callAction = false;
            if (delta > 40)
            {
                if (delta > OffsetDelta)
                {
                    callAction = true;
                }
                else
                {
                    ScrollingDownCounter++;
                    if (ScrollingDownCounter > CounterTrigger)
                        callAction = true;
                }

                if (callAction && !ScrollingDownCalled)
                {
                    Debug.WriteLine("scrolling down");

                    if (AppBar != null)
                    {
                        if (AppBar.ClosedDisplayMode == AppBarClosedDisplayMode.Compact)
                            AppBar.ClosedDisplayMode = AppBarClosedDisplayMode.Minimal;
                    }

                    if (ScrollingDown != null)
                        ScrollingDown(this, null);

                    ScrollingDownCalled = true;
                    ScrollingUpCalled = false;

                    ScrollingDownCounter = 0;
                    ScrollingUpCounter = 0;
                }
            }
            else if (delta < -40)
            {
                if (delta < -OffsetDelta || newOffset == 0)
                {
                    callAction = true;
                }
                else
                {
                    ScrollingUpCounter++;
                    if (ScrollingUpCounter > CounterTrigger)
                        callAction = true;
                }

                if (callAction && !ScrollingUpCalled)
                {
                    Debug.WriteLine("scrolling up");

                    if (AppBar != null)
                    {
                        if (AppBar.ClosedDisplayMode == AppBarClosedDisplayMode.Minimal)
                            AppBar.ClosedDisplayMode = AppBarClosedDisplayMode.Compact;
                    }

                    if (ScrollingUp != null)
                        ScrollingUp(this, null);

                    ScrollingUpCalled = true;
                    ScrollingDownCalled = false;

                    ScrollingUpCounter = 0;
                    ScrollingDownCounter = 0;
                }
            }
        }
        #endregion

        #region VisibleItems
        public int VisibleItems_FirstIdx()
        {
            var panel = this.ItemsPanelRoot as ItemsStackPanel;
            if (panel != null)
                return panel.FirstVisibleIndex;
            else
                return -1;
        }

        public int VisibleItems_LastIdx()
        {
            var panel = this.ItemsPanelRoot as ItemsStackPanel;
            if (panel != null)
                return panel.LastVisibleIndex;
            else
                return -1;
        }
        #endregion
    }
}
