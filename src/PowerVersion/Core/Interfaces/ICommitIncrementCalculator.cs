namespace PowerVersion.Core.Interfaces
{
    using PowerVersion.Core.Enums;
    using PowerVersion.Core.Models;

    /// <summary>
    /// Calculates the correct version part to increment for a given commit.
    /// </summary>
    public interface ICommitIncrementCalculator
    {
        /// <summary>
        /// Calculate the version part to increment for a commit.
        /// </summary>
        /// <param name="commit">The commit.</param>
        /// <param name="solution">The solution.</param>
        /// <param name="solutionUpdate">Whether the commit updates the solution.</param>
        /// <returns>The version part or null if the commit does not result in an increment.</returns>
        VersionPart? Calculate(GitCommit commit, string solution, bool solutionUpdate);
    }
}