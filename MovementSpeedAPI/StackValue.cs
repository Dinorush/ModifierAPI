using System;

namespace MovementSpeedAPI
{
    internal struct StackValue
    {
        public float Value
        {
            get
            {
                if (!_isDirty)
                    return _value;
                _value = UseOverride ? OverrideMod : AddMod * MultMod * MaxMod * MinMod;
                _isDirty = false;
                return _value;
            }
        }

        private float _value;
        private bool _isDirty;

        public bool UseOverride;
        public float OverrideMod;
        public float AddMod;
        public float MultMod;
        public float MaxMod;
        private bool _hasMax;
        public float MinMod;
        private bool _hasMin;

        public StackValue()
        {
            UseOverride = false;
            OverrideMod = 1f;
            AddMod = 1f;
            MultMod = 1f;
            MaxMod = 1f;
            _hasMax = false;
            MinMod = 1f;
            _hasMin = false;
            _value = 1f;
            _isDirty = false;
        }

        public StackValue(StackValue other)
        {
            UseOverride = other.UseOverride;
            OverrideMod = other.OverrideMod;
            AddMod = other.AddMod;
            MultMod = other.MultMod;
            MaxMod = other.MaxMod;
            _hasMax = other._hasMax;
            MinMod = other.MinMod;
            _hasMin = other._hasMin;
            _value = other._value;
            _isDirty = other._isDirty;
        }

        public void Add(float mod, StackLayer type)
        {
            switch (type)
            {
                case StackLayer.Override:
                    OverrideMod = mod;
                    UseOverride = true;
                    break;
                case StackLayer.Add:
                    AddMod += mod - 1;
                    break;
                case StackLayer.Multiply:
                    MultMod *= mod;
                    break;
                case StackLayer.Max:
                    MaxMod = Math.Max(GetMax(), mod);
                    _hasMax = true;
                    break;
                case StackLayer.Min:
                    MinMod = Math.Min(GetMin(), mod);
                    _hasMin = true;
                    break;
            }
            _isDirty = true;
        }

        public void Reset(StackLayer type)
        {
            switch (type)
            {
                case StackLayer.Override:
                    UseOverride = false;
                    OverrideMod = 1f;
                    break;
                case StackLayer.Add:
                    AddMod = 1f;
                    break;
                case StackLayer.Multiply:
                    MultMod = 1f;
                    break;
                case StackLayer.Max:
                    MaxMod = 1f;
                    _hasMax = false;
                    break;
                case StackLayer.Min:
                    MinMod = 1f;
                    _hasMin = false;
                    break;
            }
            _isDirty = true;
        }

        public void Reset()
        {
            UseOverride = false;
            OverrideMod = 1f;
            AddMod = 1f;
            MultMod = 1f;
            MaxMod = 1f;
            _hasMax = false;
            MinMod = 1f;
            _hasMin = false;
            _value = 1f;
            _isDirty = false;
        }

        public void Combine(StackValue other)
        {
            AddMod += other.AddMod - 1f;
            MultMod *= other.MultMod;
            MaxMod = _hasMax || other._hasMax ? Math.Max(GetMax(), other.GetMax()) : 1f;
            _hasMax |= other._hasMax;
            MinMod = _hasMin || other._hasMin ? Math.Min(GetMin(), other.GetMin()) : 1f;
            _hasMin |= other._hasMin;
            OverrideMod = UseOverride ? OverrideMod : other.OverrideMod;
            UseOverride |= other.UseOverride;
            _isDirty = true;
        }

        private readonly float GetMin() => _hasMin ? MinMod : float.MaxValue;
        private readonly float GetMax() => _hasMax ? MaxMod : float.MinValue;
    }
}
