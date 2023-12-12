namespace PowerVersion.Specs.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Bogus;
    using LibGit2Sharp;
    using PowerVersion.Core.Enums;

    /// <summary>
    /// Extensions for the <see cref="Repository"/> class.
    /// </summary>
    internal static class RepositoryExtensions
    {
        private static readonly IEnumerable<string> CommitSubjectPrefixes = new[] { "feat", "fix", "build" };

        /// <summary>
        /// Fakes a commit in the repository and returns the part incremented.
        /// </summary>
        /// <param name="repo">The repository.</param>
        /// <param name="faker">A Faker.</param>
        /// <param name="solution">The solution unique name.</param>
        /// <returns>The incremented part.</returns>
        public static VersionPart FakeCommit(this Repository repo, Faker faker, string solution)
        {
            var versionPart = faker.PickRandom(VersionPart.Major, VersionPart.Minor, VersionPart.Build);
            var bodyIncrement = versionPart != VersionPart.Build ? versionPart.ToString().ToLower() : "patch";

            var commitTitle = $"{faker.PickRandom(CommitSubjectPrefixes)}: {faker.Lorem.Sentence()}";
            var commitBody = $"+semver({solution}): {bodyIncrement}";
            var signature = new Signature(faker.Name.FullName(), faker.Internet.Email(), DateTimeOffset.Now);

            var tempFileSourcePath = Path.GetTempFileName();
            var tempFileTargetPath = Path.Combine(repo.Info.WorkingDirectory, Path.GetFileName(tempFileSourcePath));

            File.Move(tempFileSourcePath, tempFileTargetPath);
            Commands.Stage(repo, tempFileTargetPath);

            repo.Commit($"{commitTitle}\n\n{commitBody}", signature, signature);

            return versionPart;
        }

        /// <summary>
        /// Fakes a commit in the repository. The commit may or may not result in a solution version increment.
        /// </summary>
        /// <param name="repo">The repository.</param>
        /// <param name="faker">A Faker.</param>
        /// <param name="solution">The solution unique name.</param>
        /// <param name="title">An optional commit message title to override the randomly generated title.</param>
        /// <param name="body">An optional commit message to override the randomly generated message.</param>
        /// <param name="solutionCommit">Whether or not to make the commit under the solution metadata folder.</param>
        public static void FakeCommit(this Repository repo, Faker faker, string solution, string title = null, string body = null, bool? solutionCommit = null)
        {
            var commitTitle = title ?? $"{faker.PickRandom(CommitSubjectPrefixes)}: {faker.Lorem.Sentence()}";
            var commitBody = body ?? faker.Lorem.Sentences();
            var signature = new Signature(faker.Name.FullName(), faker.Internet.Email(), DateTimeOffset.Now);
            var isSolutionCommit = solutionCommit ?? faker.Random.Bool(0.8f);

            var tempFileSourcePath = Path.GetTempFileName();
            var tempFileTargetDirectory = isSolutionCommit ? Path.Combine(repo.Info.WorkingDirectory, solution, "src") : repo.Info.WorkingDirectory;
            var tempFileTargetPath = Path.Combine(tempFileTargetDirectory, Path.GetFileName(tempFileSourcePath));

            File.Move(tempFileSourcePath, tempFileTargetPath);
            Commands.Stage(repo, tempFileTargetPath);

            repo.Commit($"{commitTitle}\n\n{commitBody}", signature, signature);
        }
    }
}
