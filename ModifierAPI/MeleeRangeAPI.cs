using GameData;
using Gear;
using System;
using System.Collections.Generic;

namespace ModifierAPI
{
    public static class MeleeRangeAPI
    {
        /// <summary>
        /// The default group for modifiers.
        /// </summary>
        public const string DefaultGroup = "Default";

        private readonly static Dictionary<string, ModifierGroup> _groups = new();

        private static float _mod = 1f;
        private static float _baseRange = 0f;
        private static MeleeArchetypeDataBlock? _cachedBlock = null;
        private static RangeOverrideFunc? _rangeOverride = null;

        /// <summary>
        /// A callback that overrides how melee range is modified.
        /// </summary>
        /// <param name="baseRange">The base range of the weapon.</param>
        /// <param name="mod">The current modifier.</param>
        /// <returns>
        /// True to allow the normal range modifications to apply, false to disallow.
        /// </returns>
        public delegate bool RangeOverrideFunc(float baseRange, float mod);

        /// <summary>
        /// Overrides the callback used to modify range. See <see cref="RangeOverrideFunc"/> for callback specifications.
        /// </summary>
        public static void SetRangeOverride(RangeOverrideFunc callback) => _rangeOverride = callback;

        /// <summary>
        /// Re-applies the range modifier.
        /// </summary>
        public static void RefreshRange() => SetRange();

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

        private static IStatModifier AddModifier(float mod, StackLayer layer, string group, Dictionary<string, ModifierGroup> groupSet)
        {
            if (layer < 0 || (int)layer >= StackLayerConst.NumLayers)
                throw new ArgumentException($"Invalid layer {layer} provided.");

            if (!groupSet.TryGetValue(group, out var groupMod))
                groupSet.Add(group, groupMod = new(() => Refresh(false)));

            return groupMod.Add(mod, layer);
        }

        internal static void ApplyToWeapon(MeleeWeaponFirstPerson melee)
        {
            if (_cachedBlock != null)
                _cachedBlock.CameraDamageRayLength = _baseRange;
            _cachedBlock = melee.MeleeArchetypeData;
            _baseRange = _cachedBlock.CameraDamageRayLength;
            Refresh(force: true);
        }

        internal static void Reset()
        {
            _mod = 1f;

            foreach (var group in _groups.Values)
                group.Reset();

            Refresh(force: true);
        }

        private static void Refresh(bool force = false)
        {
            bool refresh = TryRefresh(ref _mod, _groups) || force;

            if (_cachedBlock == null) return;

            if (refresh)
                SetRange();
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

        private static void SetRange()
        {
            if (_cachedBlock == null) return;

            _cachedBlock.CameraDamageRayLength = _baseRange;
            if (_rangeOverride == null || _rangeOverride(_baseRange, _mod))
                _cachedBlock.CameraDamageRayLength *= _mod;
        }
    }
}
