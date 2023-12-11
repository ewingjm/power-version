namespace PowerVersion.UnitTests.Fakers
{
    using System.Collections.Generic;
    using Bogus;
    using PowerVersion.Core.Models;

    /// <summary>
    /// A faker for Git commits.
    /// </summary>
    internal class GitCommitFaker : Faker<GitCommit>
    {
        private static readonly IEnumerable<string> CommitPrefixes = new[] { "feat", "fix", "chore", "build" };

        /// <summary>
        /// Initializes a new instance of the <see cref="GitCommitFaker"/> class.
        /// </summary>
        public GitCommitFaker()
        {
            this.CustomInstantiator(f =>
            {
                return new GitCommit(this.GetFakeHash(f), this.GetFakeSubject(f), this.GetFakeBody(f));
            });
        }

        private string GetFakeHash(Faker f)
        {
            return f.Random.Hash(7);
        }

        private string GetFakeSubject(Faker f)
        {
            var majorCommit = f.Random.Bool(0.01f);
            var commitPrefix = string.Format("{0}{1}", f.PickRandom(CommitPrefixes), majorCommit ? "!" : string.Empty);
            var text = f.Lorem.Sentence();

            return string.Format("{0}: {1}", commitPrefix, text);
        }

        private string GetFakeBody(Faker f)
        {
            return f.Lorem.Sentences();
        }
    }
}
