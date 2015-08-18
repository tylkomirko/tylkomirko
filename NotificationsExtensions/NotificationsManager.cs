using NotificationsExtensions.BadgeContent;
using NotificationsExtensions.TileContent;
using Windows.UI.Notifications;

namespace NotificationsExtensions
{
    public static class NotificationsManager
    {
        public static void SetBadge(uint count)
        {
            object liveTile;
            Windows.Storage.ApplicationData.Current.RoamingSettings.Values.TryGetValue("LiveTile", out liveTile);

            if (liveTile != null && !(bool)liveTile)
                return;

            BadgeNumericNotificationContent badgeContent = new BadgeNumericNotificationContent(count);
            BadgeUpdateManager.CreateBadgeUpdaterForApplication().Update(badgeContent.CreateNotification());
        }

        public static void ClearBadge()
        {
            BadgeUpdateManager.CreateBadgeUpdaterForApplication().Clear();
        }

        public static void SetLiveTile()
        {
#if WINDOWS_PHONE_APP
            /* change tiles to badged ones */
            var smallTile = TileContentFactory.CreateTileSquare71x71IconWithBadge();
            smallTile.Branding = TileBranding.None;
            smallTile.ImageIcon.Src = "Assets/small_badge.png";

            var mediumTile = TileContentFactory.CreateTileSquare150x150IconWithBadge();
            mediumTile.Branding = TileBranding.Name;
            mediumTile.ImageIcon.Src = "Assets/medium_badge.png";

            var wideTile = TileContentFactory.CreateTileWide310x150IconWithBadgeAndText();
            wideTile.Branding = TileBranding.Name;
            wideTile.Square150x150Content = mediumTile;
            wideTile.ImageIcon.Src = "Assets/wide_badge.png";

            TileUpdateManager.CreateTileUpdaterForApplication().Update(smallTile.CreateNotification());
            TileUpdateManager.CreateTileUpdaterForApplication().Update(mediumTile.CreateNotification());
            TileUpdateManager.CreateTileUpdaterForApplication().Update(wideTile.CreateNotification());
#endif
        }

        public static void ClearLiveTile()
        {
            ClearBadge();
            TileUpdateManager.CreateTileUpdaterForApplication().Clear();
        }
    }
}
