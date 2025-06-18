using BepInEx;
using BepInEx.Unity.IL2CPP;
using GTFO.API;

namespace MovementSpeedAPI
{
    [BepInPlugin("Dinorush." + MODNAME, MODNAME, "1.1.0")]
    internal sealed class EntryPoint : BasePlugin
    {
        public const string MODNAME = "MovementSpeedAPI";

        public override void Load()
        {
            LevelAPI.OnLevelCleanup += OnLevelCleanup;
            Log.LogMessage("Loaded " + MODNAME);
        }

        private static void OnLevelCleanup()
        {
            MoveSpeedAPI.Reset();
        }
    }
}