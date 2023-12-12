namespace PowerVersion.Specs.StepDefinitions
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Bogus;
    using LibGit2Sharp;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using PowerVersion.Core.Enums;
    using PowerVersion.Specs.Extensions;
    using TechTalk.SpecFlow;

    /// <summary>
    /// Step bindings relating to Git.
    /// </summary>
    [Binding]
    public class GitStepDefinitions
    {
        /// <summary>
        /// The context variable key for the repository directory created by <see cref="GivenAGitRepositoryHasBeenInitalised"/>.
        /// </summary>
        public const string ContextVariableRepoDirectory = nameof(ContextVariableRepoDirectory);

        /// <summary>
        /// The context variable key for the version tagged by <see cref="GivenATagHasBeenMadeMatchingTheFormatSolutionX_X_X"/>.
        /// </summary>
        public const string ContextVariableTaggedVersion = nameof(ContextVariableTaggedVersion);

        /// <summary>
        /// The context variable key for the feature branch created by <see cref="GivenIHaveCheckedOutAFeatureBranch"/>.
        /// </summary>
        public const string ContextVariableFeatureBranchName = nameof(ContextVariableFeatureBranchName);

        /// <summary>
        /// The context variable key for the release branch created by <see cref="GivenIHaveCheckedOutAReleaseBranch"/>.
        /// </summary>
        public const string ContextVariableReleaseBranchName = nameof(ContextVariableFeatureBranchName);

        private readonly ScenarioContext scenarioCtx;
        private readonly TestContext testCtx;
        private readonly Faker faker;

        /// <summary>
        /// Initializes a new instance of the <see cref="GitStepDefinitions"/> class.
        /// </summary>
        /// <param name="scenarioCtx">The scenario context.</param>
        /// <param name="testCtx">The test context.</param>
        public GitStepDefinitions(ScenarioContext scenarioCtx, TestContext testCtx)
        {
            this.scenarioCtx = scenarioCtx ?? throw new ArgumentNullException(nameof(this.scenarioCtx));
            this.testCtx = testCtx ?? throw new ArgumentNullException(nameof(this.testCtx));
            this.faker = new Faker();
        }

        /// <summary>
        /// Initialises a Git repository.
        /// </summary>
        [Given(@"a Git repository has been initalised")]
        public void GivenAGitRepositoryHasBeenInitalised()
        {
            var repoName = Guid.NewGuid().ToString();
            var repoDirectory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), repoName));

            Repository.Init(repoDirectory.FullName);

            this.testCtx.WriteLine($"Created a Git repository at {repoDirectory.FullName}");

            this.scenarioCtx.Add(
                ContextVariableRepoDirectory,
                repoDirectory.FullName);
        }

        /// <summary>
        /// Creates a random number (greater than or equal to the minimum number provided) of commits on a branch.
        /// </summary>
        /// <param name="branchName">The name of the branch.</param>
        /// <param name="minCommits">The minimum number of commits.</param>
        [Given(@"the (\w+) branch has received (\d+) or more commits")]
        public void GivenTheBranchHasReceivedOrMoreCommits(string branchName, int minCommits)
        {
            switch (branchName)
            {
                case "feature":
                    branchName = this.scenarioCtx.GetFeatureBranchName();
                    break;
                case "release":
                    branchName = this.scenarioCtx.GetReleaseBranchName();
                    break;
            }

            var solutionName = this.scenarioCtx.GetSolutionName();

            using (var repository = new Repository(this.scenarioCtx.GetRepositoryDirectory()))
            {
                if (repository.Head.FriendlyName != branchName)
                {
                    var branch = repository.Branches[branchName] ?? repository.CreateBranch(branchName);

                    Commands.Checkout(repository, branch, new CheckoutOptions { CheckoutModifiers = CheckoutModifiers.Force });
                }

                var commitCount = this.faker.Random.Number(minCommits, minCommits + 10);

                var versionParts = new List<VersionPart>();
                for (int i = 0; i < commitCount; i++)
                {
                    versionParts.Add(repository.FakeCommit(this.faker, solutionName));
                }

                this.scenarioCtx.Add(branchName, versionParts);
            }
        }

        /// <summary>
        /// Creates a commit with a commit message increment.
        /// </summary>
        /// <param name="increments">A list of increments to randomly select from.</param>
        [Given(@"a commit has been made with '\+semver\(<solutionName>\): ' followed by any of the following in the commit message:")]
        public void GivenACommitHasBeenMadeWithFollowedByAnyOfTheFollowingInTheCommitMessage(Table increments)
        {
            using (var repository = new Repository(this.scenarioCtx.GetRepositoryDirectory()))
            {
                var commitBody = $"+semver({this.scenarioCtx.GetSolutionName()}): {this.faker.PickRandom(increments.Rows.Select(r => r[0]))}";
                this.testCtx.WriteLine($"Creating a commit with {commitBody} in the commit message body");

                repository.FakeCommit(this.faker, this.scenarioCtx.GetSolutionName(), body: commitBody);
            }
        }

        /// <summary>
        /// Creates a commit with a Conventional Commits major version increment.
        /// </summary>
        [Given(@"a commit has been made with solution metadata updates and an '!' after the Conventional Commits type in the commit subject")]
        public void GivenACommitHasBeenMadeWithSolutionMetadataUpdatesAndAnAfterTheConventionalCommitsTypeInTheCommitSubject()
        {
            using (var repository = new Repository(this.scenarioCtx.GetRepositoryDirectory()))
            {
                repository.FakeCommit(
                    this.faker,
                    this.scenarioCtx.GetSolutionName(),
                    title: $"feat!: {this.faker.Lorem.Sentence()}",
                    solutionCommit: true);
            }
        }

        /// <summary>
        /// Creates a commit with a Conventional Commits increment.
        /// </summary>
        /// <param name="conventionalCommitsType">The Conventional Commits commit type.</param>
        [Given(@"a commit has been made with solution metadata updates and a Conventional Commits type of '([^']*)'")]
        public void GivenACommitHasBeenMadeWithSolutionMetadataUpdatesAndAConventionalCommitsTypeOf(string conventionalCommitsType)
        {
            using (var repository = new Repository(this.scenarioCtx.GetRepositoryDirectory()))
            {
                repository.FakeCommit(
                    this.faker,
                    this.scenarioCtx.GetSolutionName(),
                    title: $"{conventionalCommitsType}: {this.faker.Lorem.Sentence()}",
                    solutionCommit: true);
            }
        }

        /// <summary>
        /// Creates a commit with a Conventional Commits message with a type that isn't 'feat'.
        /// </summary>
        [Given(@"a commit has been made with solution metadata updates and a Conventional Commits type in commit subject that is not 'feat'")]
        public void GivenACommitHasBeenMadeWithSolutionMetadataUpdatesAndAConventionalCommitsTypeInCommitSubjectThatIsNot()
        {
            using (var repository = new Repository(this.scenarioCtx.GetRepositoryDirectory()))
            {
                repository.FakeCommit(
                    this.faker,
                    this.scenarioCtx.GetSolutionName(),
                    title: $"{this.faker.PickRandom("fix", "build", "test", "chore")}: {this.faker.Lorem.Sentence()}",
                    solutionCommit: true);
            }
        }

        /// <summary>
        /// Creates a solution version tag.
        /// </summary>
        [Given(@"a tag has been made matching the format `<solution>/x\.x\.x`")]
        public void GivenATagHasBeenMadeMatchingTheFormatSolutionX_X_X()
        {
            var solutionName = this.scenarioCtx.GetSolutionName();
            var version = new System.Version(this.faker.Random.ReplaceNumbers("#.#.#"));

            using (var repository = new Repository(this.scenarioCtx.GetRepositoryDirectory()))
            {
                repository.ApplyTag($"{solutionName}/{version}");
            }

            this.scenarioCtx.Add(ContextVariableTaggedVersion, version);
        }

        /// <summary>
        /// Checks out a feature branch.
        /// </summary>
        [Given(@"I have checked out a feature branch")]
        public void GivenIHaveCheckedOutAFeatureBranch()
        {
            var branchName = $"feature/{this.faker.Lorem.Sentence().ToLower().Replace(' ', '-').TrimEnd('.')}";

            using (var repository = new Repository(this.scenarioCtx.GetRepositoryDirectory()))
            {
                repository.CreateBranch(branchName);
            }

            this.scenarioCtx.Add(ContextVariableFeatureBranchName, branchName);
        }

        /// <summary>
        /// Checks out a release branch.
        /// </summary>
        [Given(@"I have checked out a release branch")]
        public void GivenIHaveCheckedOutAReleaseBranch()
        {
            var branchName = $"release/{new System.Version(this.faker.Random.ReplaceNumbers("#.#.#"))}";

            using (var repository = new Repository(this.scenarioCtx.GetRepositoryDirectory()))
            {
                repository.CreateBranch(branchName);
            }

            this.scenarioCtx.Add(ContextVariableReleaseBranchName, branchName);
        }
    }
}
