using GalaSoft.MvvmLight.Ioc;
using Mirko.Common;
using Mirko.ViewModel;
using System;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko.Controls
{
    public sealed partial class HyperlinkTextBlock : UserControl
    {
        public HyperlinkTextBlock()
        {
            this.DataContext = this;

            this.InitializeComponent();
        }

        
        public string VisibleText
        {
            get { return ( string)GetValue(VisibleTextProperty); }
            set { SetValue(VisibleTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for VisibleText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VisibleTextProperty =
            DependencyProperty.Register("VisibleText", typeof( string), typeof(HyperlinkTextBlock), null);



        public string URL
        {
            get { return (string)GetValue(URLProperty); }
            set { SetValue(URLProperty, value); }
        }

        // Using a DependencyProperty as the backing store for URL.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty URLProperty =
            DependencyProperty.Register("URL", typeof(string), typeof(HyperlinkTextBlock), null);


        private async void UserControl_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            var tb = sender as HyperlinkTextBlock;
            var url = tb.URL;

            var index = url.IndexOf("wykop.pl/wpis/");
            if (index != -1)
            {
                index += 14;
                uint id = 0;

                var secondIndex = url.IndexOf('/', index);

                if (secondIndex != -1)
                    id = Convert.ToUInt32(url.Substring(index, secondIndex - index));
                else
                    id = Convert.ToUInt32(url.Substring(index));

                SimpleIoc.Default.GetInstance<MainViewModel>().GoToEntryPage.Execute(id);
            }
            else
            {
                await Launcher.LaunchUriAsync(new Uri(url));
            }
        }

        

    }
}
