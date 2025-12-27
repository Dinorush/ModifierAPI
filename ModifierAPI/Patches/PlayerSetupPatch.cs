using GameData;
using HarmonyLib;
using ModifierAPI;
using Player;

namespace ModifierAPI.Patches
{
    [HarmonyPatch(typeof(LocalPlayerAgent))]
    internal static class PlayerSetupPatch
    {
        [HarmonyPatch(nameof(LocalPlayerAgent.Setup))]
        [HarmonyPrefix]
        private static void OnAgentSetup(LocalPlayerAgent __instance)
        {
            if (!__instance.m_isSetup)
                MoveSpeedAPI.CachePlayerData(PlayerDataBlock.GetBlock(1u));
        }
    }
}
