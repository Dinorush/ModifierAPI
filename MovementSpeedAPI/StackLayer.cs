namespace MovementSpeedAPI
{
    /// <summary> Used to determine how speed modifiers stack.</summary>
    public enum StackLayer
    {
        /// <summary> All modifiers are multiplied together.</summary>
        Multiply,
        /// <summary> All modifiers are added together (i.e. [Mod_A - 1] + [Mod_B - 1] + ...).</summary>
        Add,
        /// <summary> Only the modifier with the highest value is used.</summary>
        Max,
        /// <summary> Only the modifier with the lowest value is used.</summary>
        Min,
        /// <summary> Overrides all other modifiers. If multiple overrides are applied, the chosen modifier is ambiguous.</summary>
        Override
    }
}
