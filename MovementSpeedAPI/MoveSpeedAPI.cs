using GameData;
using System;
using System.Collections.Generic;

namespace MovementSpeedAPI
{
    public static class MoveSpeedAPI
    {
        /// <summary>
        /// The default group for modifiers.
        /// </summary>
        public const string DefaultGroup = "Default";

        private readonly static int NumLayers = (int)Enum.GetValues<StackLayer>()[^1] + 1;
        private readonly static Dictionary<string, ModifierGroup> _groups = new();

        private static PlayerDataBlock _playerData = null!;
        private static float _baseWalkSpeed;
        private static float _baseRunSpeed;
        private static float _baseCrouchSpeed;
        private static float _baseAirSpeed;
        private static float _lastScale;

        /// <summary>
        /// Adds a local movement modifier, returning the modifier object.
        /// </summary>
        /// <param name="mod">The value of the modifier.</param>
        /// <param name="layer">The layer within the group to place the modifier on.</param>
        /// <param name="group">The group to put the modifier in. Layers function per-group. Separate groups are multiplied together for the final result.</param>
        /// <returns>
        /// The modifier object created.
        /// </returns>
        public static ISpeedModifier AddModifier(float mod, StackLayer layer = StackLayer.Multiply, string group = DefaultGroup)
        {
            if (layer < 0 || (int)layer >= NumLayers)
                throw new ArgumentException($"Invalid layer {layer} provided.");

            if (!_groups.TryGetValue(group, out var groupMod))
                _groups.Add(group, groupMod = new());

            return groupMod.Add(mod, layer);
        }

        internal static void Reset()
        {
            _lastScale = 1f;
            foreach (var group in _groups.Values)
                group.Reset();

            if (_playerData == null) return;

            _playerData.walkMoveSpeed = _baseWalkSpeed;
            _playerData.runMoveSpeed = _baseRunSpeed;
            _playerData.crouchMoveSpeed = _baseCrouchSpeed;
            _playerData.airMoveSpeed = _baseAirSpeed;
        }

        internal static void Refresh()
        {
            float scale = 1f;
            foreach (var group in _groups.Values)
                scale *= group.Mod.Value;

            if (_lastScale == scale || _playerData == null) return;
            _lastScale = scale;

            _playerData.walkMoveSpeed = _baseWalkSpeed * scale;
            _playerData.runMoveSpeed = _baseRunSpeed * scale;
            _playerData.crouchMoveSpeed = _baseCrouchSpeed * scale;
            _playerData.airMoveSpeed = _baseAirSpeed * scale;
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
    }
}
