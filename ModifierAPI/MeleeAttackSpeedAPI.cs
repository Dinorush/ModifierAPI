using GameData;
using Gear;
using Player;
using System;
using System.Collections.Generic;

namespace ModifierAPI
{
    public static class MeleeAttackSpeedAPI
    {
        /// <summary>
        /// The default group for modifiers.
        /// </summary>
        public const string DefaultGroup = "Default";

        private readonly static Dictionary<string, ModifierGroup> _groups = new();
        private readonly static Dictionary<string, ModifierGroup> _lightGroups = new();
        private readonly static Dictionary<string, ModifierGroup> _chargedGroups = new();
        private readonly static Dictionary<string, ModifierGroup> _pushGroups = new();

        private static float _mod = 1f;
        private static float _lightMod = 1f;
        private static float _chargedMod = 1f;
        private static float _pushMod = 1f;
        private static MeleeWeaponFirstPerson? _cachedMelee;

        /// <summary>
        /// Adds an attack speed modifier to all attack types, returning the modifier object.
        /// </summary>
        /// <param name="mod">The value of the modifier.</param>
        /// <param name="layer">The layer within the group to place the modifier on.</param>
        /// <param name="group">The group to put the modifier in. Layers function per-group. Separate groups are multiplied together for the final result.</param>
        /// <returns>
        /// The modifier object created.
        /// </returns>
        public static IStatModifier AddModifier(float mod, StackLayer layer = StackLayer.Multiply, string group = DefaultGroup) => AddModifier(mod, layer, group, _groups);

        /// <summary>
        /// Adds a light attack speed modifier, returning the modifier object.
        /// </summary>
        /// <param name="mod">The value of the modifier.</param>
        /// <param name="layer">The layer within the group to place the modifier on.</param>
        /// <param name="group">The group to put the modifier in. Layers function per-group. Separate groups are multiplied together for the final result.</param>
        /// <returns>
        /// The modifier object created.
        /// </returns>
        public static IStatModifier AddLightModifier(float mod, StackLayer layer = StackLayer.Multiply, string group = DefaultGroup) => AddModifier(mod, layer, group, _lightGroups);

        /// <summary>
        /// Adds a charged attack speed modifier, including charge time, returning the modifier object.
        /// </summary>
        /// <param name="mod">The value of the modifier.</param>
        /// <param name="layer">The layer within the group to place the modifier on.</param>
        /// <param name="group">The group to put the modifier in. Layers function per-group. Separate groups are multiplied together for the final result.</param>
        /// <returns>
        /// The modifier object created.
        /// </returns>
        public static IStatModifier AddChargedModifier(float mod, StackLayer layer = StackLayer.Multiply, string group = DefaultGroup) => AddModifier(mod, layer, group, _chargedGroups);

        /// <summary>
        /// Adds a push attack speed modifier, returning the modifier object.
        /// </summary>
        /// <param name="mod">The value of the modifier.</param>
        /// <param name="layer">The layer within the group to place the modifier on.</param>
        /// <param name="group">The group to put the modifier in. Layers function per-group. Separate groups are multiplied together for the final result.</param>
        /// <returns>
        /// The modifier object created.
        /// </returns>
        public static IStatModifier AddPushModifier(float mod, StackLayer layer = StackLayer.Multiply, string group = DefaultGroup) => AddModifier(mod, layer, group, _pushGroups);

        private static IStatModifier AddModifier(float mod, StackLayer layer, string group, Dictionary<string, ModifierGroup> groupSet)
        {
            if (layer < 0 || (int)layer >= StackLayerConst.NumLayers)
                throw new ArgumentException($"Invalid layer {layer} provided.");

            if (!groupSet.TryGetValue(group, out var groupMod))
                groupSet.Add(group, groupMod = new(() => Refresh(false)));

            return groupMod.Add(mod, layer);
        }

        internal static float GetMod(eMeleeWeaponState state)
        {
            float stateMod = state switch {
                eMeleeWeaponState.AttackMissLeft
                or eMeleeWeaponState.AttackMissRight
                or eMeleeWeaponState.AttackHitLeft
                or eMeleeWeaponState.AttackHitRight => _lightMod,

                eMeleeWeaponState.AttackChargeUpLeft
                or eMeleeWeaponState.AttackChargeUpRight
                or eMeleeWeaponState.AttackChargeReleaseLeft
                or eMeleeWeaponState.AttackChargeReleaseRight
                or eMeleeWeaponState.AttackChargeHitLeft
                or eMeleeWeaponState.AttackChargeHitRight => _chargedMod,

                eMeleeWeaponState.Push => _pushMod,
                _ => 1f
            };
            return stateMod * _mod;
        }

        internal static void ApplyToWeapon(MeleeWeaponFirstPerson melee)
        {
            _cachedMelee = melee;
            Refresh(force: true);
        }

        internal static void Reset()
        {
            _mod = 1f;
            _lightMod = 1f;
            _chargedMod = 1f;
            _pushMod = 1f;

            foreach (var group in _groups.Values)
                group.Reset();
            foreach (var group in _lightGroups.Values)
                group.Reset();
            foreach (var group in _chargedGroups.Values)
                group.Reset();
            foreach (var group in _pushGroups.Values)
                group.Reset();

            Refresh(force: true);
        }

        private static void Refresh(bool force = false)
        {
            bool refreshAll = TryRefresh(ref _mod, _groups) || force;
            bool refreshLight = TryRefresh(ref _lightMod, _lightGroups) || refreshAll;
            bool refreshCharged = TryRefresh(ref _chargedMod, _chargedGroups) || refreshAll;
            bool refreshPush = TryRefresh(ref _pushMod, _pushGroups) || refreshAll;

            if (_cachedMelee == null) return;

            if (refreshLight)
                SetLightAttackTimings(_cachedMelee);
            if (refreshCharged)
                SetChargedAttackTimings(_cachedMelee);
            if (refreshPush)
                SetPushAttackTimings(_cachedMelee);
        }

        private static bool TryRefresh(ref float mod, Dictionary<string, ModifierGroup> groups)
        {
            float scale = 1f;
            foreach (var group in groups.Values)
                scale *= group.Mod.Value;

            if (mod != scale)
            {
                mod = scale;
                return true;
            }
            return false;
        }

        private static void SetLightAttackTimings(MeleeWeaponFirstPerson melee)
        {
            float mod = 1f / (_mod * _lightMod);
            var states = melee.m_states;
            var animData = melee.MeleeAnimationData;
            CopyMeleeData(states[(int)eMeleeWeaponState.AttackMissLeft].AttackData, animData.FPAttackMissLeft, mod);
            CopyMeleeData(states[(int)eMeleeWeaponState.AttackMissRight].AttackData, animData.FPAttackMissRight, mod);
            CopyMeleeData(states[(int)eMeleeWeaponState.AttackHitLeft].AttackData, animData.FPAttackHitLeft, mod);
            CopyMeleeData(states[(int)eMeleeWeaponState.AttackHitRight].AttackData, animData.FPAttackHitRight, mod);
        }

        private static void SetChargedAttackTimings(MeleeWeaponFirstPerson melee)
        {
            float mod = 1f / (_mod * _chargedMod);
            var states = melee.m_states;
            var animData = melee.MeleeAnimationData;
            CopyMeleeData(states[(int)eMeleeWeaponState.AttackChargeReleaseLeft].AttackData, animData.FPAttackChargeUpReleaseLeft, mod);
            CopyMeleeData(states[(int)eMeleeWeaponState.AttackChargeReleaseRight].AttackData, animData.FPAttackChargeUpReleaseRight, mod);
            CopyMeleeData(states[(int)eMeleeWeaponState.AttackChargeHitLeft].AttackData, animData.FPAttackChargeUpHitLeft, mod);
            CopyMeleeData(states[(int)eMeleeWeaponState.AttackChargeHitRight].AttackData, animData.FPAttackChargeUpHitRight, mod);
        }

        private static void SetPushAttackTimings(MeleeWeaponFirstPerson melee)
        {
            float mod = 1f / (_mod * _pushMod);
            var states = melee.m_states;
            var animData = melee.MeleeAnimationData;

            CopyMeleeData(states[(int)eMeleeWeaponState.Push].AttackData, animData.FPAttackPush, mod);
        }

        private static void CopyMeleeData(MeleeAttackData data, MeleeAnimationSetDataBlock.MeleeAttackData animData, float mod = 1f)
        {
            data.m_attackLength = animData.AttackLengthTime * mod;
            data.m_attackHitTime = animData.AttackHitTime * mod;
            data.m_damageStartTime = animData.DamageStartTime * mod;
            data.m_damageEndTime = animData.DamageEndTime * mod;
            data.m_attackCamFwdHitTime = animData.AttackCamFwdHitTime * mod;
            data.m_comboEarlyTime = animData.ComboEarlyTime * mod;
        }
    }
}
