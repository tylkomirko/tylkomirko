using System;
using System.Diagnostics;
using Windows.UI.Xaml.Controls;

namespace Mirko_v2.Utils
{
    public class ScrollingDetector
    {
        public Action ScrollingDownAction { get; set; }
        public Action ScrollingUpAction { get; set; }

        private const int OffsetDelta = 24;
        private const int CounterTrigger = 6;

        private bool ScrollingDownCalled = false;
        private bool ScrollingUpCalled = false;

        private int ScrollingUpCounter = 0;
        private int ScrollingDownCounter = 0;

        private int PreviousOffset = 0;
        private int Offset = 0;

        public ScrollingDetector(ScrollViewer ScrollViewer)
        {
            if (ScrollViewer != null)
                ScrollViewer.ViewChanged += ScrollViewer_ViewChanged;
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var sV = sender as ScrollViewer;
            var newOffset = Convert.ToInt32(sV.VerticalOffset) / 2;

            PreviousOffset = Offset;
            Offset = newOffset;

            var delta = Offset - PreviousOffset;
           // Debug.WriteLine("delta: " + delta);

            bool callAction = false;
            if (delta > 2)
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

                    if (ScrollingDownAction != null)
                        ScrollingDownAction();

                    ScrollingDownCalled = true;
                    ScrollingUpCalled = false;

                    ScrollingDownCounter = 0;
                    ScrollingUpCounter = 0;
                }
            }
            else if (delta < -2)
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

                    if (ScrollingUpAction != null)
                        ScrollingUpAction();

                    ScrollingUpCalled = true;
                    ScrollingDownCalled = false;

                    ScrollingUpCounter = 0;
                    ScrollingDownCounter = 0;
                }
            }
        }
    }
}
