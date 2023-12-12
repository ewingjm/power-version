namespace PowerVersion.Core.Interfaces
{
    using System.Collections.Generic;
    using PowerVersion.Core.Models;

    /// <summary>
    /// An interface for Git.
    /// </summary>
    public interface IGitRepository
    {
        /// <summary>
        /// Get the current ref.
        /// </summary>
        /// <returns>The name of the current ref.</returns>
        string GetBranch();

        /// <summary>
        /// Get branches containing the given tag.
        /// </summary>
        /// <param name="tag">The tag to use to get branches.</param>
        /// <returns>The branches.</returns>
        IEnumerable<string> GetBranchesWithTag(string tag);

        /// <summary>
        /// Get the commit for the given ref. Defaults to the HEAD.
        /// </summary>
        /// <param name="reference">The ref.</param>
        /// <returns>The commit hash.</returns>
        GitCommit GetCommit(string reference = "HEAD");

        /// <summary>
        /// Gets a range of commits for a given Git ref.
        /// </summary>
        /// <param name="reference">The Git ref to get commits for.</param>
        /// <param name="path">The path under which to get commits.</param>
        /// <returns>The commit hashes.</returns>
        IEnumerable<GitCommit> GetCommits(string reference = "HEAD", string path = null);

        /// <summary>
        /// Gets a range of commits that exist in the <paramref name="right"/> ref but not in the <paramref name="left"/> ref.
        /// </summary>
        /// <param name="left">The ref on the left of the `..` operator.</param>
        /// <param name="right">The ref on the right of the `..` operator.</param>
        /// <param name="path">The path under which to get commits.</param>
        /// <returns>The commit hashes.</returns>
        IEnumerable<GitCommit> GetCommits(string left, string right, string path = null);

        /// <summary>
        /// Gets the most recent tag for a given ref.
        /// </summary>
        /// <param name="reference">The ref.</param>
        /// <param name="match">A pattern to match.</param>
        /// <returns>The commit hash.</returns>
        GitTag GetTag(string reference = "HEAD", string match = "*");

        /// <summary>
        /// Gets the most recent common ancestor of two refs.
        /// </summary>
        /// <param name="left">The first ref.</param>
        /// <param name="right">The second ref.</param>
        /// <returns>The commit hash.</returns>
        GitCommit GetMergeBase(string left, string right);
    }
}
