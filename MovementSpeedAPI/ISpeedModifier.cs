namespace MovementSpeedAPI
{
    /// <summary>
    /// A movement speed modifier object. Player movement speed is affected by its Mod when Active.
    /// </summary>
    public interface ISpeedModifier
    {
        /// <summary>The value of this modifier.</summary>
        /// <value>Updates player speed when modified if Active.</value>
        public float Mod { get; set; }
        /// <summary>The layer of this modifier.</summary>
        public StackLayer Layer { get; }

        /// <summary>Whether the modifier is active (in the system).</summary>
        public bool Active { get; }

        /// <summary>
        /// Enables the movement modifier (adds it back to the system).
        /// </summary>
        public void Enable();

        /// <summary>
        /// Enables the movement modifier (adds it back to the system) and sets its value.
        /// </summary>
        /// <param name="mod">The value of the modifier.</param>
        public void Enable(float mod);

        /// <summary>
        /// Disables this movement modifier (removes it from the system).
        /// </summary>
        public void Disable();
    }
}
