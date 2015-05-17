using GalaSoft.MvvmLight.Ioc;
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

namespace Mirko_v2.Pages
{
    public sealed partial class AtNotificationsPage : UserControl
    {
        public AtNotificationsPage()
        {
            this.InitializeComponent();
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var notificationVM = e.ClickedItem as NotificationViewModel;
            if (notificationVM == null) return;

            var VM = SimpleIoc.Default.GetInstance<NotificationsViewModel>();
            VM.SelectedAtNotification = notificationVM;
            VM.GoToNotification.Execute(null);
        }
    }
}
