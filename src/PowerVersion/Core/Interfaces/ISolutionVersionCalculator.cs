namespace PowerVersion.Core.Interfaces
{
    using System;

    /// <summary>
    /// An interface for calculating versions based on Git.
    /// </summary>
    public interface ISolutionVersionCalculator
    {
        /// <summary>
        /// Calculates a solution version from Git.
        /// </summary>
        /// <param name="metadataDirectory">The directory containing the solution metadata.</param>
        /// <param name="mainlineBranch">The branch to use as the mainline.</param>
        /// <param name="releaseBranchPrefix">The prefix used by release branches..</param>
        /// <returns>The calculated version.</returns>
        Version Calculate(string metadataDirectory, string mainlineBranch = "master", string releaseBranchPrefix = "release/");
    }
}
