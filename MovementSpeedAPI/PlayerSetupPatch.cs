using GameData;
using HarmonyLib;
using Player;

namespace MovementSpeedAPI
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
