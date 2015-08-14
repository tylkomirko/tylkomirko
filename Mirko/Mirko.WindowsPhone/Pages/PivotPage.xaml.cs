﻿using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Mirko.Controls;
using Mirko.Utils;
using Mirko.ViewModel;
using System;
using System.Collections.Specialized;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko.Pages
{
    public sealed partial class PivotPage : UserControl, IHaveAppBar, IDisposable
    {
        private AppHeader AppHeader;

        public ItemsPresenter ItemsPresenter;
        private Storyboard ShowPivotContent;
        private bool HasEntryAnimationPlayed = false;

        private Popup NewMirkoEntriesPopup;
        private Storyboard PopupFadeIn;
        private Storyboard PopupFadeOut;
        private bool CanShowNewEntriesPopup = false;

        private double PreviousPivotOffset;
        private const double PivotOffsetThreshold = 10;

        public PivotPage()
        {
            this.InitializeComponent();

            NewMirkoEntriesPopup = this.Resources["NewMirkoEntriesPopup"] as Popup;
            PopupFadeIn = NewMirkoEntriesPopup.Resources["PopupFadeIn"] as Storyboard;
            PopupFadeOut = NewMirkoEntriesPopup.Resources["PopupFadeOut"] as Storyboard;

            this.Resources.Remove("NewMirkoEntriesPopup");
            PivotPageGrid.Children.Add(NewMirkoEntriesPopup);

            var VM = this.DataContext as MainViewModel;
            VM.MirkoNewEntries.CollectionChanged -= MirkoNewEntries_CollectionChanged;
            VM.MirkoNewEntries.CollectionChanged += MirkoNewEntries_CollectionChanged;

            var navService = SimpleIoc.Default.GetInstance<NavigationService>();
            AppHeader = navService.GetAppHeader();
        }

        public void Dispose()
        {
            NewMirkoEntriesPopup.IsOpen = false;
            NewMirkoEntriesPopup = null;
        }

        private void MirkoNewEntries_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(MainPivot.SelectedIndex != 0) return;

            var col = sender as ObservableCollectionEx<EntryViewModel>;
            if (col.Count > 0)
            {
                var sv = MirkoListView.GetDescendant<ScrollViewer>();

                if(AppBar.IsOpen || sv.VerticalOffset == 0)
                    ShowNewEntriesPopup();

                CanShowNewEntriesPopup = true;
            }
            else
            {
                CanShowNewEntriesPopup = false;
                HideNewEntriesPopup();
            }
        }

        private void MainPivot_Loaded(object sender, RoutedEventArgs e)
        {
            if (MainPivot.Margin.Top == 0)
            {
                var appHeaderHeight = AppHeader.ActualHeight;
                var statusBarHeight = Windows.UI.ViewManagement.StatusBar.GetForCurrentView().OccludedRect.Height;
                var margin = appHeaderHeight + 2.5 * statusBarHeight;

                MainPivot.Margin = new Thickness(10, -margin, 0, 0);
#if WINDOWS_PHONE_APP
                SimpleIoc.Default.GetInstance<MainViewModel>().ListViewHeaderHeight = AppHeader.ActualHeight + statusBarHeight;
#else
                SimpleIoc.Default.GetInstance<MainViewModel>().ListViewHeaderHeight = (AppHeader.ActualHeight + statusBarHeight)*1.4;
#endif
            }

            var scroll = MainPivot.GetDescendant<ScrollViewer>();
            scroll.ViewChanged -= ScrollViewer_ViewChanged;
            scroll.ViewChanged += ScrollViewer_ViewChanged;

            var VM = this.DataContext as MainViewModel;
            if (VM.MirkoNewEntries.Count > 0 && MainPivot.SelectedIndex == 0)
                CanShowNewEntriesPopup = true;
          
            if (scroll.VerticalOffset == 0 && CanShowNewEntriesPopup)
                ShowNewEntriesPopup();

            if (ItemsPresenter == null)
            {
                ItemsPresenter = MainPivot.GetDescendant<ItemsPresenter>();
                ShowPivotContent = ItemsPresenter.Resources["FadeIn"] as Storyboard;
            }

            if (!HasEntryAnimationPlayed)
                ItemsPresenter.Opacity = 0;

            /*
            if (pivot.SelectedIndex == 0)
                App.MainViewModel.ApiAddNewEntries();
            else if (pivot.SelectedIndex == 1 && PivotHeader.Opacity != 0)
                ShowHotPopup();
             * */
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var sv = sender as ScrollViewer;
            var newOffset = sv.HorizontalOffset;
            if ((Math.Abs(PreviousPivotOffset - newOffset)) >= PivotOffsetThreshold)
                AppHeader.ShowStreams();

            PreviousPivotOffset = newOffset;
        }

        private void PivotPageGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (!HasEntryAnimationPlayed)
            {
                ShowPivotContent.Begin();
                HasEntryAnimationPlayed = true;
            }

            var popupGrid = NewMirkoEntriesPopup.Child as Grid;
            var horizontal = Window.Current.Bounds.Right - popupGrid.Width - 22;
            NewMirkoEntriesPopup.HorizontalOffset = horizontal;
            NewMirkoEntriesPopup.IsOpen = true;
        }

        private void ListView_ScrollingDown(object sender, EventArgs e)
        {
            var currentPage = MainPivot.SelectedIndex;
            if (currentPage == 0)
                HideNewEntriesPopup();
            else if (currentPage == 1)
                HideTimeSpanIndicatorPopup();
        }

        private void ListView_ScrollingUp(object sender, EventArgs e)
        {
            var currentPage = MainPivot.SelectedIndex;
            if (currentPage == 0 && CanShowNewEntriesPopup)
                ShowNewEntriesPopup();
            else if (currentPage == 1)
                ShowTimeSpanIndicatorPopup();
        }

        private void MainPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var currentPage = MainPivot.SelectedIndex;
            if (currentPage == 0)
            {
                App.TelemetryClient.TrackPageView("PivotPage-Mirko");

                AppBar.MakeButtonVisible("refresh");
                if (TimeSpanIndicatorPopup.IsOpen)
                    HideTimeSpanIndicatorPopup();

                SimpleIoc.Default.GetInstance<MainViewModel>().MirkoEntries.Start();
                SimpleIoc.Default.GetInstance<MainViewModel>().HotEntries.ForceStop();
                SimpleIoc.Default.GetInstance<MainViewModel>().FavEntries.ForceStop();
                SimpleIoc.Default.GetInstance<MainViewModel>().MyEntries.ForceStop();
            }
            else if (currentPage == 1)
            {
                App.TelemetryClient.TrackPageView("PivotPage-Hot");

                HideNewEntriesPopup();
                AppBar.MakeButtonInvisible("refresh");
                ShowTimeSpanIndicatorPopup();

                SimpleIoc.Default.GetInstance<MainViewModel>().MirkoEntries.ForceStop();
                SimpleIoc.Default.GetInstance<MainViewModel>().HotEntries.Start();
                SimpleIoc.Default.GetInstance<MainViewModel>().FavEntries.ForceStop();
                SimpleIoc.Default.GetInstance<MainViewModel>().MyEntries.ForceStop();
            } 
            else if(currentPage == 2)
            {
                App.TelemetryClient.TrackPageView("PivotPage-Fav");

                HideNewEntriesPopup();
                AppBar.MakeButtonInvisible("refresh");
                if (TimeSpanIndicatorPopup.IsOpen)
                    HideTimeSpanIndicatorPopup();

                SimpleIoc.Default.GetInstance<MainViewModel>().MirkoEntries.ForceStop();
                SimpleIoc.Default.GetInstance<MainViewModel>().HotEntries.ForceStop();
                SimpleIoc.Default.GetInstance<MainViewModel>().FavEntries.Start();
                SimpleIoc.Default.GetInstance<MainViewModel>().MyEntries.ForceStop();
            }
            else if (currentPage == 3)
            {
                App.TelemetryClient.TrackPageView("PivotPage-My");

                HideNewEntriesPopup();
                AppBar.MakeButtonInvisible("refresh");
                if (TimeSpanIndicatorPopup.IsOpen)
                    HideTimeSpanIndicatorPopup();

                SimpleIoc.Default.GetInstance<MainViewModel>().MirkoEntries.ForceStop();
                SimpleIoc.Default.GetInstance<MainViewModel>().HotEntries.ForceStop();
                SimpleIoc.Default.GetInstance<MainViewModel>().FavEntries.ForceStop();
                SimpleIoc.Default.GetInstance<MainViewModel>().MyEntries.Start();
            }
        }

        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            var VM = this.DataContext as MainViewModel;
            var lv = sender as ListViewEx;

            lv.AppBar = AppBar;

            ObservableCollectionEx<EntryViewModel> items = null;
            string tag = (string)lv.Tag;
            if (tag == "LV0")
                items = VM.MirkoEntries;
            else if (tag == "LV1")
                items = VM.HotEntries;
            else if (tag == "LV2")
                items = VM.FavEntries;
            else if (tag == "LV3")
                items = VM.MyEntries;

            var idx = VM.IndexToScrollTo;
            if (idx != -1 && items != null && items.Count - 1 >= idx)
            {
                lv.ScrollIntoView(items[idx], ScrollIntoViewAlignment.Leading);
                VM.IndexToScrollTo = -1;
            }
        }

#region Popups
        private void ShowNewEntriesPopup()
        {
            PopupFadeIn.Begin();
        }

        private void HideNewEntriesPopup()
        {
            this.PopupFadeOut.Begin();
        }

        private void ShowTimeSpanIndicatorPopup()
        {
            this.TimeSpanIndicatorGrid.Width = Window.Current.Bounds.Width;

            this.TimeSpanIndicatorPopup.IsOpen = true;
            this.TimeSpanIndicatorFadeIn.Begin();
        }

        private void HideTimeSpanIndicatorPopup()
        {
            this.TimeSpanIndicatorPopup.IsOpen = false;
            this.TimeSpanIndicatorFadeOut.Begin();
        }

        private void TimeSpanIndicatorGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ShowTimeSpanSelectionPopup();
        }

        private void TimeSpanSelectionPopup_Loaded(object sender, RoutedEventArgs e)
        {
            Windows.Phone.UI.Input.HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
            Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;
        }

        private void TimeSpanSelectionPopup_Unloaded(object sender, RoutedEventArgs e)
        {
            Windows.Phone.UI.Input.HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
        }

        private void HardwareButtons_BackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            if(TimeSpanSelectionPopup.IsOpen)
            {
                e.Handled = true;
                HideTimeSpanSelectionPopup();
            }
        }

        private void TimeSpanSelectionListBox_Loaded(object sender, RoutedEventArgs e)
        {
            var selectedIndex = TimeSpanSelectionListBox.SelectedIndex;
            var panel = TimeSpanSelectionListBox.ItemsPanelRoot;

            for (int i = 0; i < panel.Children.Count; i++)
            {
                var lvItem = panel.Children[i] as ListViewItem;
                var tb = lvItem.GetDescendant<TextBlock>();

                if(i == selectedIndex)
                    tb.Foreground = App.Current.Resources["HashtagForeground"] as SolidColorBrush;
                else
                    tb.Foreground = new SolidColorBrush(Windows.UI.Colors.White);
            }
        }

        private async void TimeSpanSelectionListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HideTimeSpanSelectionPopup();

            var mainVM = this.DataContext as MainViewModel;
            mainVM.HotEntries.ClearAll();

            if(HotListView.ItemsSource != null) // forgive me for this dirty hack. it's Satya's fault.
                await HotListView.LoadMoreItemsAsync();
        }

        private void ShowTimeSpanSelectionPopup()
        {
            AppBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            this.TimeSpanSelectionListBox.Width = Window.Current.Bounds.Width;
            this.TimeSpanSelectionListBox.Height = 5000;//Window.Current.Bounds.Height;

            this.TimeSpanSelectionPopup.IsOpen = true;
            //this.TimeSpanSelectionFadeIn.Begin();

            var mainVM = this.DataContext as MainViewModel;
            mainVM.CanGoBack = false;
        }

        private void HideTimeSpanSelectionPopup()
        {
            AppBar.Visibility = Windows.UI.Xaml.Visibility.Visible;

            this.TimeSpanSelectionPopup.IsOpen = false;
            //this.TimeSpanSelectionFadeIn.Begin();

            var mainVM = this.DataContext as MainViewModel;
            mainVM.CanGoBack = true;
        }
#endregion

#region AppBar
        private CommandBar AppBar = null;

        public CommandBar CreateCommandBar()
        {
            var c = new CommandBar();
            var up = new AppBarButton()
            {
                Icon = new SymbolIcon(Symbol.Up),
                Label = "w górę",
            };
            up.Click += ScrollUpButton_Click;

            var add = new AppBarButton()
            {
                Icon = new SymbolIcon(Symbol.Add),
                Label = "nowy",
            };
            add.SetBinding(AppBarButton.CommandProperty, new Binding()
            {
                Source = this.DataContext as MainViewModel,
                Path = new PropertyPath("AddNewEntryCommand"),
            });
            add.SetBinding(AppBarButton.VisibilityProperty, new Binding()
            {
                Source = SimpleIoc.Default.GetInstance<SettingsViewModel>(),
                Path = new PropertyPath("UserInfo"),
                Mode = BindingMode.OneWay,
                Converter = App.Current.Resources["NullToVisibility"] as IValueConverter,
            });

            var refresh = new AppBarButton()
            {
                Label = "odśwież",
                Tag = "refresh",
#if WINDOWS_PHONE_APP
                Icon = new BitmapIcon() { UriSource = new Uri("ms-appx:///Assets/refresh.png") },
#else
                Icon = new SymbolIcon(Symbol.Refresh),
#endif
            };
            refresh.SetBinding(AppBarButton.CommandProperty, new Binding()
            {
                Source = this.DataContext as MainViewModel,
                Path = new PropertyPath("RefreshMirkoEntries"),
            });

            var profile = new AppBarButton()
            {
                Label = "mój profil",
            };
            profile.SetBinding(AppBarButton.VisibilityProperty, new Binding()
            {
                Source = SimpleIoc.Default.GetInstance<SettingsViewModel>(),
                Path = new PropertyPath("UserInfo"),
                Mode = BindingMode.OneWay,
                Converter = App.Current.Resources["NullToVisibility"] as IValueConverter,
            });
            profile.SetBinding(AppBarButton.CommandProperty, new Binding()
            {
                Source = this.DataContext as MainViewModel,
                Path = new PropertyPath("GoToYourProfile"),
            });

            var settings = new AppBarButton()
            {
                Label = "ustawienia",
            };
            settings.SetBinding(AppBarButton.CommandProperty, new Binding() 
            {
                Source = this.DataContext as MainViewModel,
                Path = new PropertyPath("SettingsCommand"),
            });

            var logout = new AppBarButton()
            {
                Label = "wyloguj",
            };
            logout.SetBinding(AppBarButton.CommandProperty, new Binding()
            {
                Source = this.DataContext as MainViewModel,
                Path = new PropertyPath("LogInOutCommand"),
            });
            logout.SetBinding(AppBarButton.VisibilityProperty, new Binding()
            {
                Source = SimpleIoc.Default.GetInstance<SettingsViewModel>(),
                Path = new PropertyPath("UserInfo"),
                Mode = BindingMode.OneWay,
                Converter = App.Current.Resources["NullToVisibility"] as IValueConverter,
            });

            var login = new AppBarButton()
            {
                Label = "zaloguj",
            };
            login.SetBinding(AppBarButton.CommandProperty, new Binding()
            {
                Source = this.DataContext as MainViewModel,
                Path = new PropertyPath("LogInOutCommand"),
            });
            login.SetBinding(AppBarButton.VisibilityProperty, new Binding()
            {
                Source = logout,
                Path = new PropertyPath("Visibility"),
                Converter = App.Current.Resources["InvertVisibility"] as IValueConverter,
            });

            var blacklist = new AppBarButton()
            {
                Label = "czarnolisto",
            };
            blacklist.SetBinding(AppBarButton.CommandProperty, new Binding()
            {
                Source = this.DataContext as MainViewModel,
                Path = new PropertyPath("GoToBlacklistPage"),
            });
            blacklist.SetBinding(AppBarButton.VisibilityProperty, new Binding()
            {
                Source = SimpleIoc.Default.GetInstance<SettingsViewModel>(),
                Path = new PropertyPath("UserInfo"),
                Mode = BindingMode.OneWay,
                Converter = App.Current.Resources["NullToVisibility"] as IValueConverter,
            });

            var donation = new AppBarButton()
            {
                Label = "podziękuj",
            };
            donation.SetBinding(AppBarButton.CommandProperty, new Binding()
            {
                Source = this.DataContext as MainViewModel,
                Path = new PropertyPath("GoToDonationPage"),
            });

            c.PrimaryCommands.Add(add);
            c.PrimaryCommands.Add(refresh);
            c.PrimaryCommands.Add(up);
            c.SecondaryCommands.Add(profile);
            c.SecondaryCommands.Add(settings);
            c.SecondaryCommands.Add(blacklist);
            c.SecondaryCommands.Add(donation);
            c.SecondaryCommands.Add(login);
            c.SecondaryCommands.Add(logout);
            AppBar = c;

            return c;
        }

        private void ScrollUpButton_Click(object sender, RoutedEventArgs e)
        {
            ScrollViewer sv = null;
            var index = MainPivot.SelectedIndex;

            if (index == 0)
                sv = MirkoListView.GetDescendant<ScrollViewer>();
            else if (index == 1)
                sv = HotListView.GetDescendant<ScrollViewer>();

            if(sv != null)
                sv.ChangeView(null, 0.0, null);
        }
#endregion
    }
}