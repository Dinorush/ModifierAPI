using BepInEx;
using BepInEx.Unity.IL2CPP;
using GTFO.API;
using HarmonyLib;

namespace MovementSpeedAPI
{
    [BepInPlugin("Dinorush." + MODNAME, MODNAME, "1.1.1")]
    internal sealed class EntryPoint : BasePlugin
    {
        public const string MODNAME = "MovementSpeedAPI";

        public override void Load()
        {
            new Harmony(MODNAME).PatchAll();
            LevelAPI.OnLevelCleanup += MoveSpeedAPI.Reset;
            Log.LogMessage("Loaded " + MODNAME);
        }
    }
}