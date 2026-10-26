public static class SteamManager
{
    public static bool Initialized => SteamAchievements.IsReady;
}
