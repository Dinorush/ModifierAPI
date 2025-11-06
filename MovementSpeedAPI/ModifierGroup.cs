using System;
using System.Collections.Generic;

namespace MovementSpeedAPI
{
    internal class ModifierGroup
    {
        private readonly static int NumLayers = (int)Enum.GetValues<StackLayer>()[^1] + 1;

        private readonly HashSet<SpeedModifier>[] _layers = new HashSet<SpeedModifier>[NumLayers];
        private StackValue _mod = new();
        public StackValue Mod => _mod;

        public ISpeedModifier Add(float mod, StackLayer layer = StackLayer.Multiply)
        {
            var modifier = new SpeedModifier(mod, layer, this);
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

        private HashSet<SpeedModifier> GetLayer(StackLayer layer) => _layers[(int)layer] ?? (_layers[(int)layer] = new());

        private void Refresh(StackLayer layer)
        {
            if (_layers[(int)layer] == null) return;

            _mod.Reset(layer);
            foreach (var modifier in GetLayer(layer))
                _mod.Add(modifier.Mod, layer);
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
