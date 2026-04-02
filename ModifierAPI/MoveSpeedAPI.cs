using GameData;
using System;
using System.Collections.Generic;

namespace ModifierAPI
{
    public static class MoveSpeedAPI
    {
        /// <summary>
        /// The default group for modifiers.
        /// </summary>
        public const string DefaultGroup = "Default";

        private readonly static Dictionary<string, ModifierGroup> _groups = new();
        private readonly static Dictionary<string, ModifierGroup> _walkGroups = new();
        private readonly static Dictionary<string, ModifierGroup> _crouchGroups = new();
        private readonly static Dictionary<string, ModifierGroup> _sprintGroups = new();
        private readonly static Dictionary<string, ModifierGroup> _airGroups = new();

        private static PlayerDataBlock _playerData = null!;
        private static float _baseWalkSpeed;
        private static float _baseRunSpeed;
        private static float _baseCrouchSpeed;
        private static float _baseAirSpeed;

        /// <summary>
        /// Adds a local movement modifier, returning the modifier object. Speed modifiers are disabled on level cleanup.
        /// </summary>
        /// <param name="mod">The value of the modifier.</param>
        /// <param name="layer">The layer within the group to place the modifier on.</param>
        /// <param name="group">The group to put the modifier in. Layers function per-group. Separate groups are multiplied together for the final result.</param>
        /// <returns>
        /// The modifier object created.
        /// </returns>
        public static IStatModifier AddModifier(float mod, StackLayer layer = StackLayer.Multiply, string group = DefaultGroup)
        {
            return AddModifier(_groups, mod, layer, group);
        }

        /// <summary>
        /// Adds a local walk speed modifier, returning the modifier object. Speed modifiers are disabled on level cleanup.
        /// </summary>
        /// <param name="mod">The value of the modifier.</param>
        /// <param name="layer">The layer within the group to place the modifier on.</param>
        /// <param name="group">The group to put the modifier in. Layers function per-group. Separate groups are multiplied together for the final result.</param>
        /// <returns>
        /// The modifier object created.
        /// </returns>
        public static IStatModifier AddWalkModifier(float mod, StackLayer layer = StackLayer.Multiply, string group = DefaultGroup)
        {
            return AddModifier(_walkGroups, mod, layer, group);
        }

        /// <summary>
        /// Adds a local sprint speed modifier, returning the modifier object. Speed modifiers are disabled on level cleanup.
        /// </summary>
        /// <param name="mod">The value of the modifier.</param>
        /// <param name="layer">The layer within the group to place the modifier on.</param>
        /// <param name="group">The group to put the modifier in. Layers function per-group. Separate groups are multiplied together for the final result.</param>
        /// <returns>
        /// The modifier object created.
        /// </returns>
        public static IStatModifier AddSprintModifier(float mod, StackLayer layer = StackLayer.Multiply, string group = DefaultGroup)
        {
            return AddModifier(_sprintGroups, mod, layer, group);
        }

        /// <summary>
        /// Adds a local walk speed modifier, returning the modifier object. Speed modifiers are disabled on level cleanup.
        /// </summary>
        /// <param name="mod">The value of the modifier.</param>
        /// <param name="layer">The layer within the group to place the modifier on.</param>
        /// <param name="group">The group to put the modifier in. Layers function per-group. Separate groups are multiplied together for the final result.</param>
        /// <returns>
        /// The modifier object created.
        /// </returns>
        public static IStatModifier AddCrouchModifier(float mod, StackLayer layer = StackLayer.Multiply, string group = DefaultGroup)
        {
            return AddModifier(_crouchGroups, mod, layer, group);
        }

        /// <summary>
        /// Adds a local walk speed modifier, returning the modifier object. Speed modifiers are disabled on level cleanup.
        /// </summary>
        /// <param name="mod">The value of the modifier.</param>
        /// <param name="layer">The layer within the group to place the modifier on.</param>
        /// <param name="group">The group to put the modifier in. Layers function per-group. Separate groups are multiplied together for the final result.</param>
        /// <returns>
        /// The modifier object created.
        /// </returns>
        public static IStatModifier AddAirModifier(float mod, StackLayer layer = StackLayer.Multiply, string group = DefaultGroup)
        {
            return AddModifier(_airGroups, mod, layer, group);
        }

        private static IStatModifier AddModifier(Dictionary<string, ModifierGroup> modGroup, float mod, StackLayer layer, string group)
        {
            if (layer < 0 || (int)layer >= StackLayerConst.NumLayers)
                throw new ArgumentException($"Invalid layer {layer} provided.");

            if (!modGroup.TryGetValue(group, out var groupMod))
                modGroup.Add(group, groupMod = new(Refresh));

            return groupMod.Add(mod, layer);
        }

        internal static void Reset()
        {
            foreach (var group in _groups.Values)
                group.Reset();
            foreach (var group in _walkGroups.Values)
                group.Reset();
            foreach (var group in _crouchGroups.Values)
                group.Reset();
            foreach (var group in _sprintGroups.Values)
                group.Reset();
            foreach (var group in _airGroups.Values)
                group.Reset();

            if (_playerData == null) return;

            _playerData.walkMoveSpeed = _baseWalkSpeed;
            _playerData.runMoveSpeed = _baseRunSpeed;
            _playerData.crouchMoveSpeed = _baseCrouchSpeed;
            _playerData.airMoveSpeed = _baseAirSpeed;
        }

        internal static void Refresh()
        {
            if (_playerData == null) return;

            float scale = GetMod(_groups);
            _playerData.walkMoveSpeed = _baseWalkSpeed * scale * GetMod(_walkGroups);
            _playerData.runMoveSpeed = _baseRunSpeed * scale * GetMod(_sprintGroups);
            _playerData.crouchMoveSpeed = _baseCrouchSpeed * scale * GetMod(_crouchGroups);
            _playerData.airMoveSpeed = _baseAirSpeed * scale * GetMod(_airGroups);
        }

        internal static void CachePlayerData(PlayerDataBlock data)
        {
            if (_playerData != null && _playerData.Pointer == data.Pointer) return;

            _playerData = data;
            _baseWalkSpeed = _playerData.walkMoveSpeed;
            _baseRunSpeed = _playerData.runMoveSpeed;
            _baseCrouchSpeed = _playerData.crouchMoveSpeed;
            _baseAirSpeed = _playerData.airMoveSpeed;
        }

        private static float GetMod(Dictionary<string, ModifierGroup> modGroup)
        {
            float mod = 1f;
            foreach (var group in modGroup.Values)
                mod *= group.Mod.Value;
            return mod;
        }
    }
}
