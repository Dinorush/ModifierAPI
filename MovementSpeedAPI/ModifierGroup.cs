using System;
using System.Collections.Generic;
using System.Linq;

namespace MovementSpeedAPI
{
    internal class ModifierGroup
    {
        private readonly static int NumLayers = (int)Enum.GetValues<StackLayer>()[^1] + 1;

        private readonly HashSet<ISpeedModifier>[] _layers = new HashSet<ISpeedModifier>[NumLayers];
        private bool _useOverride = false;
        private float _overrideMod = 1f;
        private float _maxMod = 1f;
        private float _minMod = 1f;
        private float _multMod = 1f;
        private float _addMod = 1f;
        public float Mod { get; private set; } = 1f;

        public ISpeedModifier Add(float mod, StackLayer layer = StackLayer.Multiply)
        {
            var modifier = new SpeedModifier(mod, layer, this);
            GetLayer(layer).Add(modifier);
            Refresh(layer);
            return modifier;
        }

        public void Reset()
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
            Mod = 1f;
        }

        private HashSet<ISpeedModifier> GetLayer(StackLayer layer) => _layers[(int)layer] ?? (_layers[(int)layer] = new());

        private void Refresh(StackLayer layer)
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

            float lastMod = Mod;
            Mod = _useOverride ? _overrideMod : _maxMod * _minMod * _multMod * _addMod;
            if (lastMod != Mod)
                MoveSpeedAPI.Refresh();
        }

        public class SpeedModifier : ISpeedModifier
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
                        _parent.Refresh(Layer);
                }
            }

            public StackLayer Layer { get; }

            public bool Active { get; internal set; }

            private readonly ModifierGroup _parent;

            public SpeedModifier(float mod, StackLayer layer, ModifierGroup parent)
            {
                Active = true;
                _mod = mod;
                Layer = layer;
                _parent = parent;
            }

            public void Enable()
            {
                if (Active) return;
                Active = true;
                _parent.GetLayer(Layer).Add(this);
                _parent.Refresh(Layer);
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
                _parent.GetLayer(Layer).Remove(this);
                _parent.Refresh(Layer);
            }
        }
    }
}
