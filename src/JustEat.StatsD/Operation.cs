namespace JustEat.StatsD
{
    /// <summary>
    /// Defines the available gauge operations.
    /// </summary>
    public enum Operation
    {
        /// <summary>
        /// Set the gauge value.
        /// </summary>
        Set,

        /// <summary>
        /// Increment the gauge value.
        /// </summary>
        Increment,

        /// <summary>
        /// Decrement the gauge value.
        /// </summary>
        Decrement,
    }
}
