namespace PowerVersion.Core.Models
{
    using System;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Information captured by a solution version tag (e.g. pwr_Version_Core/1.0.0).
    /// </summary>
    public class SolutionVersionTag
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionVersionTag"/> class.
        /// </summary>
        /// <param name="tag">The Git tag to parse.</param>
        public SolutionVersionTag(GitTag tag)
        {
            if (tag is null)
            {
                throw new ArgumentNullException(nameof(tag));
            }

            this.Hash = tag.CommitHash;

            var match = Regex.Match(tag.Name, $"^(?<solution>\\w+)/(?<version>(?<major>\\d+)(\\.?(?<minor>\\d+))?(\\.?(?<patch>\\d+))?(\\.?(?<revision>\\d+))?)$");
            if (!match.Success)
            {
                throw new ArgumentException($"The provided tag is not a valid solution version tag: {tag.Name}.", nameof(tag));
            }

            this.Solution = match.Groups["solution"].ToString();
            this.Version = new Version(match.Groups["version"].ToString());
        }

        /// <summary>
        /// Gets the commit tagged.
        /// </summary>
        public string Hash { get; private set; }

        /// <summary>
        /// Gets the solution tagged.
        /// </summary>
        public string Solution { get; private set; }

        /// <summary>
        /// Gets the solution version tagged.
        /// </summary>
        public Version Version { get; private set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{this.Solution}/{this.Version}";
        }
    }
}
