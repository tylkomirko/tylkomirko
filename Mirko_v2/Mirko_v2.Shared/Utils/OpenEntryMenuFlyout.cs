﻿using Microsoft.Xaml.Interactivity;
using Mirko_v2.ViewModel;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using WykopAPI.Models;

namespace Mirko_v2.Utils
{
    public class OpenEntryMenuFlyout : DependencyObject, IAction
    {
        public object Execute(object sender, object parameter)
        {
            var src = sender as Controls.Entry;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(src.GetDescendant<Grid>("EntryGrid"));

            var entryData = (src.DataContext as EntryViewModel).Data;

            var userInfo = App.ApiService.UserInfo;
            if (userInfo == null || userInfo.UserName == null)
                return null;

            var myUserName = userInfo.UserName;

            Action<string> makeFlyoutItemVisible = (name) =>
            {
                var items = (flyoutBase as MenuFlyout).Items;
                foreach (var i in items)
                {
                    if (i.Tag != null && i.Tag.ToString() == name)
                    {
                        i.Visibility = Visibility.Visible;
                        break;
                    }
                }
            };

            Action<string> makeFlyoutItemInvisible = (name) =>
            {
                var items = (flyoutBase as MenuFlyout).Items;
                foreach (var i in items)
                {
                    if (i.Tag != null && i.Tag.ToString() == name)
                    {
                        i.Visibility = Visibility.Collapsed;
                        break;
                    }
                }
            };

            if(entryData.Voted)
            {
                makeFlyoutItemInvisible("plus");
                makeFlyoutItemVisible("unplus");
            }

            if (entryData.Favourite)
            {
                makeFlyoutItemInvisible("favourite");
                makeFlyoutItemVisible("unfavourite");
            }

            if(entryData.AuthorName != myUserName)
            {
                makeFlyoutItemInvisible("separator"); // FIXME
            }

            flyoutBase.ShowAt(src);

            return null;
        }
    }

}
