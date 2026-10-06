namespace PaperCar
{
    /// <summary>
    /// Source of blow/thrust intensity. Swap this for a networked input later.
    /// </summary>
    public interface IThrustInput
    {
        /// <summary>Smoothed 0–1 intensity after the noise gate (not an edge trigger).</summary>
        float Intensity { get; }

        /// <summary>
        /// True on the frame a blow should fire an impulse.
        /// <paramref name="intensity"/> is 0–1 after clamping.
        /// </summary>
        bool TryGetThrust(out float intensity);
    }
}
