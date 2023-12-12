namespace PowerVersion.Core.Models
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// A Git tag.
    /// </summary>
    public class GitTag
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GitTag"/> class.
        /// </summary>
        /// <param name="name">The name of the tag.</param>
        /// <param name="commitHash">The hash of the commit.</param>
        [JsonConstructor]
        public GitTag(string name, string commitHash)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new System.ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));
            }

            if (string.IsNullOrEmpty(commitHash))
            {
                throw new System.ArgumentException($"'{nameof(commitHash)}' cannot be null or empty.", nameof(commitHash));
            }

            this.Name = name;
            this.CommitHash = commitHash;
        }

        /// <summary>
        /// Gets the commit hash of the tag.
        /// </summary>
        public string CommitHash { get; private set; }

        /// <summary>
        /// Gets the name of the tag.
        /// </summary>
        public string Name { get; private set; }
    }
}
