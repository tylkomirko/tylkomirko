﻿using GalaSoft.MvvmLight.Messaging;
using Mirko_v2.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Mirko_v2.Utils;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Mirko_v2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            this.InitializeComponent();

            Messenger.Default.Register<LoginShowFlyoutMessage>(this, LoginShowFlyoutAction);
        }

        private void LoginShowFlyoutAction(LoginShowFlyoutMessage obj)
        {
            if(obj.FlyoutType == LoginShowFlyoutMessage.FlyoutTypeEnum.Error)
            {
                ShowErrorFlyoutWithText(obj.ErrorMessage);
            }
            else
            {
                ShowPermissionFlyout(obj.PermissionsTitle, obj.Permissions);
            }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private void UsernameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.UsernameBox.Text.Length > 0 && this.PasswordBox.Password.Length > 0)
                LoginButton.IsEnabled = true;
            else
                LoginButton.IsEnabled = false;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.UsernameBox.Text.Length > 0 && this.PasswordBox.Password.Length > 0)
                LoginButton.IsEnabled = true;
            else
                LoginButton.IsEnabled = false;
        }

        private void UsernameBox_Loaded(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            tb.Focus(FocusState.Programmatic);
        }

        private void ShowErrorFlyoutWithText(string txt)
        {
            if (txt == null) return;
            var flyout = this.Resources["ErrorFlyout"] as Flyout;
            var tb = flyout.Content as TextBlock;
            tb.Text = txt;

            flyout.ShowAt(this);
        }

        private void ShowPermissionFlyout(string title, List<string> items)
        {
            var flyout = this.Resources["PermissionFlyout"] as Flyout;
            var sp = flyout.Content as StackPanel;
            var permissionTitleTB = sp.GetDescendant<TextBlock>("PermissionNeededTB");
            permissionTitleTB.Text = title;

            var permissionsLV = sp.GetDescendant<ListView>("PermissionListView");
            permissionsLV.ItemsSource = items;

            flyout.ShowAt(this);
        }

        private async void ShowProgressIndicator()
        {
            var statusBar = StatusBar.GetForCurrentView();
            await statusBar.ShowAsync();
            await statusBar.ProgressIndicator.ShowAsync();
        }

        private async void HideProgressIndicator()
        {
            var statusBar = StatusBar.GetForCurrentView();
            await statusBar.ProgressIndicator.HideAsync();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(UsernameBox.Text))
            {
                ShowErrorFlyoutWithText("Podaj nazwę użytkownika.");
                return;
            }
            else if (string.IsNullOrEmpty(PasswordBox.Password))
            {
                ShowErrorFlyoutWithText("Podaj hasło.");
                return;
            }

            var VM = this.DataContext as LoginViewModel;
            VM.LoginCommand.Execute(null);
        }

        private void GrantPermissionButton_Click(object sender, RoutedEventArgs e)
        {
            var flyout = this.Resources["PermissionFlyout"] as Flyout;
            flyout.Hide();
        }
    }
}
