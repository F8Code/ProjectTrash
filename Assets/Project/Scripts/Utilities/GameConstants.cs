public static class GameConstants
{
    public static class Layer
    {
        public const int Default = 0;
        public const int TransparentFX = 1;
        public const int IgnoreRaycast = 2;
        public const int Player = 3;
        public const int Water = 4;
        public const int UI = 5;
        public const int Trash = 6;
        public const int TrashDespawnPlane = 7;
        public const int Obstcale = 29;
        public const int Interaction = 31;
    }

    public static class Scene
    {
        public const string Bootstrapper = "0- Bootstrapper";
        public const string MainMenu = "1- MainMenu";
        public const string Game = "2- GamePlay";
    }

    public static class AudioMixer
    {
        public const string Master = "Master";
        public const string Background = "Background";
        public const string SFX = "SFX";
    }

    public static class Canvas
    {
        public const string GameOverPanel = "GameOverPanel_Canvas";
        public const string LeaderboardPanel = "Leaderboard_Canvas";
        public const string PauseMenu = "UICanvas_PauseMenu";
    }
}