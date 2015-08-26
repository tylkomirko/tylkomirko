using Microsoft.Xaml.Interactivity;
using Mirko.ViewModel;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
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
                menuFlyout.MakeItemInvisible("favourite");
                if (comment.RootEntryAuthor == myUserName)
                {
                    menuFlyout.MakeItemVisible("separator");
                    menuFlyout.MakeItemVisible("delete");
                }
            }
            else if ((entryData as Entry).Favourite)
            {
                menuFlyout.MakeItemInvisible("favourite");
                menuFlyout.MakeItemVisible("unfavourite");
            }

            if (entryData.AuthorName == myUserName)
            {
                menuFlyout.MakeItemVisible("separator");
                menuFlyout.MakeItemVisible("delete");
                if(DateTime.UtcNow - entryData.Date.Subtract(App.OffsetUTCInPoland) < EditTime)
                    menuFlyout.MakeItemVisible("edit");
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
