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
        private static float _baseWalkSpeed = 0;
        private static float _baseRunSpeed;
        private static float _baseCrouchSpeed;
        private static float _baseAirSpeed;

        /// <summary>
        /// Adds a local movement modifier, returning the modifier object.
        /// </summary>
        /// <param name="mod">The value of the modifier.</param>
        /// <param name="layer">The layer within the group to place the modifier on.</param>
        /// <param name="groupName">The group to put the modifier in. Layers function per-group. Separate groups are multiplied together for the final result.</param>
        /// <returns>
        /// The modifier object created.
        /// </returns>
        public static ISpeedModifier AddModifier(float mod, StackLayer layer = StackLayer.Multiply, string groupName = DefaultGroup)
        {
            if (layer < 0 || (int)layer >= NumLayers)
                throw new ArgumentException($"Invalid layer {layer} provided.");

            if (!_groups.TryGetValue(groupName, out var group))
                _groups.Add(groupName, group = new());

            return group.Add(mod, layer);
        }

        internal static void Reset()
        {
            foreach (var group in _groups.Values)
                group.Reset();
        }

        internal static void Refresh()
        {
            float mod = 1f;
            foreach (var group in _groups.Values)
                mod *= group.Mod;

            CacheBaseSpeed();
            _playerData.walkMoveSpeed = _baseWalkSpeed * mod;
            _playerData.runMoveSpeed = _baseRunSpeed * mod;
            _playerData.crouchMoveSpeed = _baseCrouchSpeed * mod;
            _playerData.airMoveSpeed = _baseAirSpeed * mod;
        }

        private static void CacheBaseSpeed()
        {
            if (_baseWalkSpeed != 0) return;

            _playerData = PlayerDataBlock.GetBlock(1u);
            _baseWalkSpeed = _playerData.walkMoveSpeed;
            _baseRunSpeed = _playerData.runMoveSpeed;
            _baseCrouchSpeed = _playerData.crouchMoveSpeed;
            _baseAirSpeed = _playerData.airMoveSpeed;
        }
    }
}
