using BepInEx;
using BepInEx.Unity.IL2CPP;
using GTFO.API;
using HarmonyLib;

namespace ModifierAPI
{
    [BepInPlugin("Dinorush." + MODNAME, MODNAME, "1.2.1")]
    internal sealed class EntryPoint : BasePlugin
    {
        public const string MODNAME = "ModifierAPI";

        public override void Load()
        {
            new Harmony(MODNAME).PatchAll();
            LevelAPI.OnLevelCleanup += MoveSpeedAPI.Reset;
            Log.LogMessage("Loaded " + MODNAME);
        }
    }
}