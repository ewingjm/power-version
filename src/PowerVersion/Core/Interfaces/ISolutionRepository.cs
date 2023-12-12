namespace PowerVersion.Core.Interfaces
{
    using PowerVersion.Core.Models;

    /// <summary>
    /// An interface for a solution repository.
    /// </summary>
    public interface ISolutionRepository
    {
        /// <summary>
        /// Gets a solution.
        /// </summary>
        /// <param name="metadataDirectory">The path to the solution metadata directory.</param>
        /// <returns>The <see cref="SolutionManifest"/>.</returns>
        SolutionManifest GetManifest(string metadataDirectory);
    }
}
