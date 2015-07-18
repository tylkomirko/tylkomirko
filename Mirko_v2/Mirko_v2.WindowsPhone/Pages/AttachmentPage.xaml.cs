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
    public sealed partial class AttachmentPage : UserControl, IHaveAppBar
    {
        public AttachmentPage()
        {
            this.InitializeComponent();
        }

        public CommandBar CreateCommandBar()
        {
            var ok = new AppBarButton()
            {
                Label = "ok",
                Icon = new SymbolIcon(Symbol.Accept),
            };

            ok.SetBinding(AppBarButton.CommandProperty, new Binding()
            {
                Source = this.DataContext as NewEntryViewModel,
                Path = new PropertyPath("AcceptAttachments"),
            });

            var c = new CommandBar();
            c.PrimaryCommands.Add(ok);

            return c;
        }
    }
}
