using ModifierAPI.Structs;
using System;
using System.Collections.Generic;

namespace ModifierAPI
{
    internal class ModifierGroup
    {
        private readonly HashSet<StatModifier>[] _layers = new HashSet<StatModifier>[StackLayerConst.NumLayers];
        private readonly Action _onRefresh;
        private StackValue _mod = new();
        public StackValue Mod => _mod;

        public ModifierGroup(Action onRefresh) => _onRefresh = onRefresh;

        public IStatModifier Add(float mod, StackLayer layer = StackLayer.Multiply)
        {
            var modifier = new StatModifier(mod, layer, this);
            modifier.Enable();
            return modifier;
        }

        public void Reset()
        {
            foreach (var layer in _layers)
            {
                if (layer == null) continue;

                foreach (var modifier in layer)
                    modifier.Active = false;
                layer.Clear();
            }

            _mod.Reset();
        }

        private HashSet<StatModifier> GetLayer(StackLayer layer) => _layers[(int)layer] ?? (_layers[(int)layer] = new());

        private void Refresh(StackLayer layer)
        {
            if (_layers[(int)layer] == null) return;

            _mod.Reset(layer);
            foreach (var modifier in GetLayer(layer))
                _mod.Add(modifier.Mod, layer);
            _onRefresh();
        }

        public class StatModifier : IStatModifier
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

            public StatModifier(float mod, StackLayer layer, ModifierGroup parent)
            {
                Active = false;
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
