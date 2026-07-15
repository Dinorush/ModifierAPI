using Gear;
using HarmonyLib;
using System;

namespace ModifierAPI.Patches
{
    [HarmonyPatch(typeof(MeleeWeaponFirstPerson))]
    internal static class MeleePatches
    {
        [HarmonyPatch(nameof(MeleeWeaponFirstPerson.SetupMeleeAnimations))]
        [HarmonyWrapSafe]
        [HarmonyPriority(Priority.High)]
        [HarmonyPostfix]
        private static void Post_MeleeSetup(MeleeWeaponFirstPerson __instance)
        {
            MeleeAttackSpeedAPI.ApplyToWeapon(__instance);
            MeleeRangeAPI.ApplyToWeapon(__instance);
        }

        private static MWS_AttackLight? _lightLeft;
        private static MWS_AttackLight? _lightRight;
        private static IntPtr _cachedPtr = IntPtr.Zero;
        [HarmonyPatch(nameof(MeleeWeaponFirstPerson.ChangeState))]
        [HarmonyWrapSafe]
        [HarmonyPostfix]
        private static void Post_MeleeChangeState(MeleeWeaponFirstPerson __instance, eMeleeWeaponState newState)
        {
            if (_cachedPtr != __instance.Pointer)
            {
                _cachedPtr = __instance.Pointer;
                _lightLeft = __instance.m_states[(int)eMeleeWeaponState.AttackMissLeft].TryCast<MWS_AttackLight>()!;
                _lightRight = __instance.m_states[(int)eMeleeWeaponState.AttackMissRight].TryCast<MWS_AttackLight>()!;
            }

            float mod = MeleeAttackSpeedAPI.GetMod(newState);
            switch (newState)
            {
                case eMeleeWeaponState.AttackMissLeft:
                    _lightLeft!.m_wantedNormalSpeed = mod;
                    _lightLeft.m_wantedChargeSpeed = mod * 0.3f;
                    break;
                case eMeleeWeaponState.AttackMissRight:
                    _lightRight!.m_wantedNormalSpeed = mod;
                    _lightRight.m_wantedChargeSpeed = mod * 0.3f;
                    break;
                case eMeleeWeaponState.AttackHitLeft:
                case eMeleeWeaponState.AttackHitRight:
                    __instance.WeaponAnimator.speed = mod;
                    break;
                case eMeleeWeaponState.AttackChargeReleaseLeft:
                case eMeleeWeaponState.AttackChargeReleaseRight:
                    __instance.WeaponAnimator.speed = mod;
                    break;
                case eMeleeWeaponState.Push: // Push sets speed based on stamina
                    __instance.WeaponAnimator.speed *= mod;
                    break;
            }
        }

        private static float _cacheChargeDiff = -1f;
        private static float _lastMod = 1f;
        [HarmonyPatch(typeof(MWS_ChargeUp), nameof(MWS_ChargeUp.Enter))]
        [HarmonyWrapSafe]
        [HarmonyPostfix]
        private static void ChargeCallback(MWS_ChargeUp __instance)
        {
            AdjustChargeTime(__instance);
        }

        internal static void AdjustChargeTime(MWS_ChargeUp chargeUp)
        {
            float mod = MeleeAttackSpeedAPI.GetChargingMod();
            if (_lastMod == mod) return;

            var animData = chargeUp.m_weapon.MeleeAnimationData;
            chargeUp.m_maxDamageTime *= _lastMod / mod;
            chargeUp.m_startTime += chargeUp.m_elapsed / _lastMod - chargeUp.m_elapsed / mod;
            if (_cacheChargeDiff != -1)
            {
                animData.AutoAttackTime += _cacheChargeDiff;
                animData.AutoAttackWarningTime += _cacheChargeDiff;
            }

            _cacheChargeDiff = chargeUp.m_maxDamageTime * mod - chargeUp.m_maxDamageTime;
            animData.AutoAttackTime -= _cacheChargeDiff;
            animData.AutoAttackWarningTime -= _cacheChargeDiff;
        }

        [HarmonyPatch(typeof(MWS_ChargeUp), nameof(MWS_ChargeUp.Exit))]
        [HarmonyWrapSafe]
        [HarmonyPostfix]
        private static void RestoreAutoAttackTimings(MWS_ChargeUp __instance)
        {
            _lastMod = 1f;
            if (_cacheChargeDiff == -1) return;

            var animData = __instance.m_weapon.MeleeAnimationData;
            animData.AutoAttackTime += _cacheChargeDiff;
            animData.AutoAttackWarningTime += _cacheChargeDiff;
            _cacheChargeDiff = -1;
        }
    }
}
