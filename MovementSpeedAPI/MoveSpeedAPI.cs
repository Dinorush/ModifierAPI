using GameData;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MovementSpeedAPI
{
    public static class MoveSpeedAPI
    {
        private readonly static int NumLayers = (int)Enum.GetValues<StackLayer>()[^1] + 1;
        private readonly static HashSet<ISpeedModifier>[] _layers = new HashSet<ISpeedModifier>[NumLayers];

        private static PlayerDataBlock _playerData = null!;
        private static float _baseWalkSpeed = 0;
        private static float _baseRunSpeed;
        private static float _baseCrouchSpeed;
        private static float _baseAirSpeed;

        private static bool _useOverride = false;
        private static float _overrideMod = 1f;
        private static float _maxMod = 1f;
        private static float _minMod = 1f;
        private static float _multMod = 1f;
        private static float _addMod = 1f;
        private static float _mod = 1f;

        /// <summary>
        /// Adds a local movement modifier, returning the modifier object.
        /// </summary>
        /// <param name="mod">The value of the modifier.</param>
        /// <param name="layer">The layer to place the modifier on.</param>
        /// <returns>
        /// The modifier object created.
        /// </returns>
        public static ISpeedModifier AddModifier(float mod, StackLayer layer = StackLayer.Multiply)
        {
            if (layer < 0 || (int) layer >= NumLayers)
                throw new ArgumentException($"Invalid layer {layer} provided.");

            var modifier = new SpeedModifier(mod, layer);
            GetLayer(layer).Add(modifier);
            Refresh(layer);
            return modifier;
        }

        internal static void Reset()
        {
            foreach (var layer in _layers)
            {
                if (layer == null) continue;

                foreach (var modifier in layer)
                    ((SpeedModifier)modifier).Active = false;
                layer.Clear();
            }

            _useOverride = false;
            _overrideMod = 1f;
            _maxMod = 1f;
            _minMod = 1f;
            _multMod = 1f;
            _addMod = 1f;
            _mod = 1f;
        }

        private static HashSet<ISpeedModifier> GetLayer(StackLayer layer) => _layers[(int)layer] ?? (_layers[(int)layer] = new());

        private static void Refresh(StackLayer layer)
        {
            switch (layer)
            {
                case StackLayer.Override:
                    var modLayer = GetLayer(StackLayer.Override);
                    if (modLayer.Count > 0)
                    {
                        _overrideMod = modLayer.First().Mod;
                        _useOverride = true;
                    }
                    else
                    {
                        _overrideMod = 1f;
                        _useOverride = false;
                    }
                    break;
                case StackLayer.Max:
                    _maxMod = GetLayer(StackLayer.Max).Max(modifier => modifier.Mod);
                    break;
                case StackLayer.Min:
                    _minMod = GetLayer(StackLayer.Min).Min(modifier => modifier.Mod);
                    break;
                case StackLayer.Multiply:
                    _multMod = 1f;
                    foreach (var modifier in GetLayer(StackLayer.Multiply))
                        _multMod *= modifier.Mod;
                    break;
                case StackLayer.Add:
                    _addMod = 1f;
                    foreach (var modifier in GetLayer(StackLayer.Add))
                        _addMod += modifier.Mod - 1f;
                    break;
            }

            float lastMod = _mod;
            _mod = _useOverride ? _overrideMod : _maxMod * _minMod * _multMod * _addMod;
            if (lastMod == _mod) return;

            CacheBaseSpeed();
            _playerData.walkMoveSpeed = _baseWalkSpeed * _mod;
            _playerData.runMoveSpeed = _baseRunSpeed * _mod;
            _playerData.crouchMoveSpeed = _baseCrouchSpeed * _mod;
            _playerData.airMoveSpeed = _baseAirSpeed * _mod;
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

        internal class SpeedModifier : ISpeedModifier
        {
            private float _mod = 0f;
            
            public float Mod
            {
                get => _mod;
                set
                {
                    var oldMod = _mod;
                    _mod = value;
                    if (Active && oldMod != _mod)
                        Refresh(Layer);
                }
            }

            public StackLayer Layer { get; }

            public bool Active { get; internal set; }

            public SpeedModifier(float mod, StackLayer layer)
            {
                Active = true;
                _mod = mod;
                Layer = layer;
            }

            public void Enable()
            {
                if (Active) return;
                Active = true;
                GetLayer(Layer).Add(this);
                Refresh(Layer);
            }

            public void Enable(float mod)
            {
                Enable();
                Mod = mod;
            }

            public void Disable()
            {
                if (!Active) return;
                Active = false;
                GetLayer(Layer).Remove(this);
                Refresh(Layer);
            }
        }
    }
}
