namespace CMetalsWS.Services
{
    public interface IGaugeWeightResolver
    {
        /// <summary>
        /// Gets the weight in pounds per square foot for a given gauge identifier.
        /// </summary>
        /// <param name="gauge">The gauge identifier (e.g., "18GA", "20GA").</param>
        /// <returns>The weight per square foot, or 0 if not found.</returns>
        decimal GetWeightPerSquareFoot(string gauge);
    }
}
