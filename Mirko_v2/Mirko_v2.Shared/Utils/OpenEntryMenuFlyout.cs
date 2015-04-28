using Microsoft.Xaml.Interactivity;
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

            var entry = src.DataContext as Entry;
            var comment = src.DataContext as EntryComment;

            var userInfo = App.ApiService.UserInfo;
            if (userInfo == null || userInfo.UserName == null)
                return null;

            if (userInfo != null && userInfo.UserName != null)
            {
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

                /*
                if (src.RootEntry != null)
                {
                    if (comment != null && comment.author == myUserName && !comment.deleted)
                    {
                        makeFlyoutItemVisible("delete");
                        makeFlyoutItemInvisible("plus");
                        makeFlyoutItemInvisible("favourite");

                        var time = DateTime.Parse(comment.date, null, System.Globalization.DateTimeStyles.RoundtripKind);
                        if(DateTime.Now - time < new TimeSpan(0, 10, 0))
                            makeFlyoutItemVisible("edit");
                    }
                    else if (comment != null && src.RootEntry.author == myUserName && !comment.deleted)
                    {
                        makeFlyoutItemVisible("delete");
                        makeFlyoutItemInvisible("favourite");
                    }
                }
                else
                {
                    if (entry != null && !entry.deleted && entry.user_favorite)
                    {
                        makeFlyoutItemInvisible("favourite");
                        makeFlyoutItemVisible("unfavourite");
                    }
                    else if (entry != null && entry.author == myUserName && !entry.deleted)
                    {
                        makeFlyoutItemVisible("edit");
                        makeFlyoutItemVisible("delete");
                        makeFlyoutItemInvisible("plus");

                        var time = DateTime.Parse(entry.date, null, System.Globalization.DateTimeStyles.RoundtripKind);
                        if ((entry.comment_count == 0) && (DateTime.Now - time < new TimeSpan(0, 10, 0)))
                            makeFlyoutItemVisible("edit");
                    }
                    else if (comment != null && comment.author == myUserName && !comment.deleted)
                    {
                        makeFlyoutItemVisible("delete");
                        makeFlyoutItemInvisible("plus");
                        makeFlyoutItemInvisible("favourite");

                        var time = DateTime.Parse(comment.date, null, System.Globalization.DateTimeStyles.RoundtripKind);
                        if (DateTime.Now - time < new TimeSpan(0, 10, 0))
                            makeFlyoutItemVisible("edit");
                    }
                }*/

            }
            flyoutBase.ShowAt(src);

            return null;
        }
    }

}
