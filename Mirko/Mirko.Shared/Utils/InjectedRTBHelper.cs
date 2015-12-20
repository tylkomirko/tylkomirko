using GalaSoft.MvvmLight.Ioc;
using Mirko.Common;
using Mirko.ViewModel;
using Windows.UI.Xaml.Controls;

namespace Mirko.Utils
{
    public interface IReceiveRTBClicks
    {
        void HashtagTapped(string tag, TextBlock tb);
        void ProfileTapped(string username);
    }

    public static class InjectedRTBHelper
    {
        public static void PrepareHashtagFlyout(ref MenuFlyout mf, string tag)
        {
            if (App.ApiService.UserInfo == null)
            {
                MenuFlyoutUtils.MakeItemInvisible(ref mf, "observeTag");
                MenuFlyoutUtils.MakeItemInvisible(ref mf, "unobserveTag");
                MenuFlyoutUtils.MakeItemInvisible(ref mf, "blacklistTag");
            }
            else
            {
                var observedTags = SimpleIoc.Default.GetInstance<CacheViewModel>().ObservedHashtags;
                if (observedTags.Contains(tag))
                {
                    MenuFlyoutUtils.MakeItemInvisible(ref mf, "observeTag");
                    MenuFlyoutUtils.MakeItemVisible(ref mf, "unobserveTag");
                }
                else
                {
                    MenuFlyoutUtils.MakeItemVisible(ref mf, "observeTag");
                    MenuFlyoutUtils.MakeItemInvisible(ref mf, "unobserveTag");
                }
            }

            var blacklistedTags = SimpleIoc.Default.GetInstance<BlacklistViewModel>().Tags;
            if (blacklistedTags.Contains(tag))
            {
                MenuFlyoutUtils.MakeItemInvisible(ref mf, "blacklistTag");
                MenuFlyoutUtils.MakeItemVisible(ref mf, "unblacklistTag");
            }
            else
            {
                MenuFlyoutUtils.MakeItemVisible(ref mf, "blacklistTag");
                MenuFlyoutUtils.MakeItemInvisible(ref mf, "unblacklistTag");
            }
        }

        public static void GoToProfilePage(string username)
        {
            var profilesVM = SimpleIoc.Default.GetInstance<ProfilesViewModel>();
            profilesVM.GoToProfile.Execute(username);
        }
    }
}
