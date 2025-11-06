using UnityEngine;

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

    public static class Tag
    {
        public const string Player = "EventHandlers";
        public const string Enemy = "Obstcale";
        public const string Interactable = "Interactable";
    }

    public static class Scene
    {
        public const string Bootstrapper = "0- Bootstrapper";
        public const string MainMenu = "1- Mainmenu";
        public const string Game = "2- Gameplay";
    }

    public static class PlayerAnimation
    {
        public const string OnGround = "OnGround";
        public const string Slide = "Slide";
        public const string Forward = "Forward";
        public const string Turn = "Turn";
        public const string Die = "Die";
        public const string VerticalSpeed = "VerticalSpeed";

        public static class StringToHash
        {
            public static readonly int OnGroundHash = Animator.StringToHash(OnGround);
            public static readonly int SlideHash = Animator.StringToHash(Slide);
            public static readonly int ForwardHash = Animator.StringToHash(Forward);
            public static readonly int TurnHash = Animator.StringToHash(Turn);
            public static readonly int DieHash = Animator.StringToHash(Die);
            public static readonly int VerticalSpeedHash = Animator.StringToHash(VerticalSpeed);
        }
    }

    public static class ShaderProperties
    {
        public const string Alpha = "_Alpha";
    }

    public static class Event
    {
        public static class Name
        {
            public const string Gameover = "Gameover";
        }

        public static class Channel
        {
            public const string GameStarts = "GameStarts";
            public const string Coin = "Coin";
            public const string Interact = "Interact";
            public const string Die = "Die";

            public const string Pause = "Pause";
            public const string Resume = "Resume";
            public const string Restart = "Restart";

            public const string LoadingProgress = "LoadingProgress";
            public const string SceneLoaded = "SceneLoaded";
            public const string SceneUnloaded = "SceneUnloaded";
            public const string SceneGroupLoaded = "SceneGroupLoaded";

            public const string MainmenuLoaded = "MainmenuLoaded";
        }
    }

    public static class AudioMixer
    {
        public const string Master = "Master";
        public const string Background = "Background";
        public const string SFX = "SFX";
    }
}