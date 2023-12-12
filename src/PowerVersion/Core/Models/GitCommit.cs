namespace PowerVersion.Core.Models
{
    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    /// A Git commit.
    /// </summary>
    public class GitCommit : IEquatable<GitCommit>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GitCommit"/> class.
        /// </summary>
        /// <param name="hash">The commit hash.</param>
        /// <param name="subject">The commit subject.</param>
        /// <param name="body">The commit body.</param>
        [JsonConstructor]
        public GitCommit(string hash, string subject, string body)
        {
            if (string.IsNullOrEmpty(hash))
            {
                throw new ArgumentException($"'{nameof(hash)}' cannot be null or empty.", nameof(hash));
            }

            if (string.IsNullOrEmpty(subject))
            {
                throw new ArgumentException($"'{nameof(subject)}' cannot be null or empty.", nameof(subject));
            }

            this.Hash = hash;
            this.Subject = subject;
            this.Body = body;
        }

        /// <summary>
        /// Gets the commit hash.
        /// </summary>
        public string Hash { get; private set; }

        /// <summary>
        /// Gets the commit subject.
        /// </summary>>
        public string Subject { get; private set; }

        /// <summary>
        /// Gets the commit body.
        /// </summary>>
        public string Body { get; private set; }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as GitCommit);
        }

        /// <summary>
        /// Checks if this commit is equal to another commit.
        /// </summary>
        /// <param name="other">The other commits.</param>
        /// <returns>True if the commits are equal (i.e. the hash is the same).</returns>
        public bool Equals(GitCommit other)
        {
            if (other == null)
            {
                return false;
            }

            return this.Hash == other.Hash;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return this.Hash.GetHashCode();
        }
    }
}
