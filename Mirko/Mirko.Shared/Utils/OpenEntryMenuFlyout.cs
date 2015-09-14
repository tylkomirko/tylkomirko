using Microsoft.Xaml.Interactivity;
using Mirko.Controls;
using Mirko.ViewModel;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using WykopSDK.API.Models;

namespace Mirko.Utils
{
    public static class MenuFlyoutEx
    {
        public static void MakeItemVisible(this MenuFlyout mf, string tag)
        {
            if (string.IsNullOrEmpty(tag)) return;

            foreach (var i in mf.Items)
            {
                if (i.Tag.ToString() == tag)
                {
                    i.Visibility = Visibility.Visible;
                    break;
                }
            }
        }

        public static void MakeItemInvisible(this MenuFlyout mf, string tag)
        {
            if (string.IsNullOrEmpty(tag)) return;

            foreach (var i in mf.Items)
            {
                if (i.Tag.ToString() == tag)
                {
                    i.Visibility = Visibility.Collapsed;
                    break;
                }
            }
        }

        public static void EnableItem(this MenuFlyout mf, string tag)
        {
            if (string.IsNullOrEmpty(tag)) return;

            foreach (var item in mf.Items)
            {
                if(item is MenuFlyoutEntryPanel)
                {
                    var panel = item as MenuFlyoutEntryPanel;
                    if (tag == "edit")
                        panel.EnableEditButton = true;
                    else if (tag == "delete")
                        panel.EnableDeleteButton = true;
                    else if (tag == "favourite")
                        panel.EnableFavButton = true;

                    break;
                }
            }
        }

        public static void DisableItem(this MenuFlyout mf, string tag)
        {
            if (string.IsNullOrEmpty(tag)) return;

            foreach (var item in mf.Items)
            {
                if (item is MenuFlyoutEntryPanel)
                {
                    var panel = item as MenuFlyoutEntryPanel;
                    if (tag == "edit")
                        panel.EnableEditButton = false;
                    else if (tag == "delete")
                        panel.EnableDeleteButton = false;
                    else if (tag == "favourite")
                        panel.EnableFavButton = false;

                    break;
                }
            }
        }
    }

    public class OpenEntryMenuFlyout : DependencyObject, IAction
    {
        public static TimeSpan EditTime = new TimeSpan(0, 10, 0);

        public object Execute(object sender, object parameter)
        {
            var src = sender as Controls.Entry;
            var entryGrid = src.GetDescendant<Grid>("EntryGrid");
            var rtb = src.GetDescendant<RichTextBlock>();

            var menuFlyout = FlyoutBase.GetAttachedFlyout(entryGrid) as MenuFlyout;
            var entry = src.DataContext as EntryBaseViewModel;
            var comment = src.DataContext as CommentViewModel;
            var entryData = entry.DataBase;

            var userInfo = App.ApiService.UserInfo;
            if (userInfo == null || userInfo.UserName == null)
                return null;

            var myUserName = userInfo.UserName;

            if(entryData.Voted)
            {
                menuFlyout.MakeItemInvisible("plus");
                menuFlyout.MakeItemVisible("unplus");
            }

            if (comment != null)
            {
               menuFlyout.DisableItem("favourite");
               if (comment.RootEntryAuthor == myUserName)
                    menuFlyout.EnableItem("delete");
            }

            if (entryData.AuthorName == myUserName)
            {
                menuFlyout.MakeItemInvisible("plus");
                menuFlyout.MakeItemInvisible("unplus");
                menuFlyout.EnableItem("delete");
                if(DateTime.UtcNow - entryData.Date.Subtract(App.OffsetUTCInPoland) < EditTime)
                    menuFlyout.EnableItem("edit");
            }

            if (entryData.VoteCount == 0 || entry.ShowVoters)
                menuFlyout.MakeItemInvisible("voters");
            else
                menuFlyout.MakeItemVisible("voters");

            menuFlyout.ShowAt(rtb);

            return null;
        }
    }

}
