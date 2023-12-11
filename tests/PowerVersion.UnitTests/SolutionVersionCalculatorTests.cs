namespace PowerVersion.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Bogus;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using PowerVersion.Core.Enums;
    using PowerVersion.Core.Interfaces;
    using PowerVersion.Core.Models;
    using PowerVersion.UnitTests.Fakers;

    /// <summary>
    /// Unit tests for the <see cref="SolutionVersionCalculator"/> class.
    /// </summary>
    [TestClass]
    public class SolutionVersionCalculatorTests
    {
        private readonly Mock<ISolutionRepository> solutionRepo;
        private readonly Mock<IGitRepository> gitRepo;
        private readonly Mock<ICommitIncrementCalculator> incrementCalculator;

        private readonly Faker faker;
        private readonly Faker<GitCommit> commitFaker;

        private readonly SolutionVersionCalculator calculator;

        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionVersionCalculatorTests"/> class.
        /// </summary>
        public SolutionVersionCalculatorTests()
        {
            this.solutionRepo = new Mock<ISolutionRepository>();
            this.gitRepo = new Mock<IGitRepository>();
            this.incrementCalculator = new Mock<ICommitIncrementCalculator>();
            this.faker = new Faker();
            this.commitFaker = new GitCommitFaker();

            this.SetupMocksWithDefaults();

            this.calculator = new SolutionVersionCalculator(this.solutionRepo.Object, this.gitRepo.Object, this.incrementCalculator.Object);
        }

        /// <summary>
        /// Asserts that an exception is thrown when a solution repository is not provided to the constructor.
        /// </summary>
        [TestMethod]
        public void Constructor_SolutionRepositoryNotProvided_Throws()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                _ = new SolutionVersionCalculator(null, this.gitRepo.Object, this.incrementCalculator.Object);
            });
        }

        /// <summary>
        /// Asserts that an exception is thrown when a Git repository is not provided to the constructor.
        /// </summary>
        [TestMethod]
        public void Constructor_GitRepositoryNotProvided_Throws()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                _ = new SolutionVersionCalculator(this.solutionRepo.Object, null, this.incrementCalculator.Object);
            });
        }

        /// <summary>
        /// Asserts that an exception is thrown when a version part calculator is not provided to the constructor.
        /// </summary>
        [TestMethod]
        public void Constructor_PartCalculatorNotProvided_Throws()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                _ = new SolutionVersionCalculator(this.solutionRepo.Object, this.gitRepo.Object, null);
            });
        }

        /// <summary>
        /// Asserts that if the current commit is tagged with a solution version tag that the tagged version is returned.
        /// </summary>
        [TestMethod]
        public void Calculate_CurrentCommitIsTaggedWithSolutionVersionTag_ReturnsTaggedSolutionVersion()
        {
            var expectedVersion = new Version("2.0.0");
            var solutionName = "pwr_Version_Core";
            var commits = this.SetupCommits(mainlineCommits: 3, nonMainlineCommits: 0, metadataCommits: 0);
            this.SetupSolutionManifest(new SolutionManifest(solutionName, new Version(0, 0, 0)));
            this.SetupCommitsToCalculate(commits);
            this.SetupVersionTag(new GitTag($"{solutionName}/{expectedVersion}", commits.Last().Hash));

            var actualVersion = this.calculator.Calculate(this.faker.System.DirectoryPath());

            Assert.AreEqual(expectedVersion, actualVersion);
        }

        /// <summary>
        /// Asserts that the a feature branch with a major increment as the largest commit increment will result in a major increment to the mainline version.
        /// </summary>
        [TestMethod]
        public void Calculate_FeatureBranchWithMajorLargestIncrement_ReturnsMainlineVersionWithMajorIncrementedByOne()
        {
            this.SetupBranch("feature/my-feature-branch");
            this.SetupCommits(mainlineCommits: 1, nonMainlineCommits: 3, metadataCommits: 0);
            this.SetupVersionIncrements(VersionPart.Minor, VersionPart.Build, VersionPart.Revision, VersionPart.Major);

            var actualVersion = this.calculator.Calculate(this.faker.System.DirectoryPath());

            Assert.AreEqual(new Version(1, 0, 0), actualVersion);
        }

        /// <summary>
        /// Asserts that a feature branch with a minor increment as the largest commit increment will result in a minor increment to the mainline version.
        /// </summary>
        [TestMethod]
        public void Calculate_FeatureBranchMinorLargestIncrement_ReturnsMainlineVersionWithMinorIncrementedByOne()
        {
            this.SetupBranch("feature/my-feature-branch");
            this.SetupCommits(mainlineCommits: 1, nonMainlineCommits: 3, metadataCommits: 0);
            this.SetupVersionIncrements(VersionPart.Minor, VersionPart.Build, VersionPart.Minor, VersionPart.Build);

            var actualVersion = this.calculator.Calculate(this.faker.System.DirectoryPath());

            Assert.AreEqual(new Version(0, 2, 0), actualVersion);
        }

        /// <summary>
        /// Asserts that a feature branch with a build increment as the largest commit increment will result in a build increment to the mainline version.
        /// </summary>
        [TestMethod]
        public void Calculate_FeatureBranchBuildLargestIncrement_ReturnsMainlineVersionWithBuildIncrementedByOne()
        {
            this.SetupBranch("feature/my-feature-branch");
            this.SetupCommits(mainlineCommits: 1, nonMainlineCommits: 3, metadataCommits: 0);
            this.SetupVersionIncrements(VersionPart.Minor, VersionPart.Build, VersionPart.Build, VersionPart.Build);

            var actualVersion = this.calculator.Calculate(this.faker.System.DirectoryPath());

            Assert.AreEqual(new Version(0, 1, 1), actualVersion);
        }

        /// <summary>
        /// Asserts that a release branch will increment the revision number of the mainline version by the count of commits that result in any sort of increment.
        /// </summary>
        [TestMethod]
        public void Calculate_ReleaseBranch_ReturnsMainlineVersionWithRevisionIncrementedByCountOfReleaseBranchCommitsThatIncrement()
        {
            this.SetupBranch("release/1.0");
            this.SetupCommits(mainlineCommits: 1, nonMainlineCommits: 3, metadataCommits: 0);
            this.SetupVersionIncrements(VersionPart.Minor, VersionPart.Major, null, VersionPart.Build);

            var actualVersion = this.calculator.Calculate(this.faker.System.DirectoryPath());

            Assert.AreEqual(new Version(0, 1, 0, 2), actualVersion);
        }

        /// <summary>
        /// Asserts that a mainline branch will calculate the version by incrementing the initial verison by each commit.
        /// </summary>
        [TestMethod]
        public void Calculate_MainlineBranchWithoutTag_ReturnsManifestVersionIncrementedByEachCommit()
        {
            this.SetupBranch("master");
            this.SetupSolutionManifest(new SolutionManifest("pwr_Version_Core", new Version(3, 0, 0)));
            this.SetupCommits(mainlineCommits: 5, nonMainlineCommits: 0, metadataCommits: 0);
            this.SetupVersionIncrements(VersionPart.Minor, VersionPart.Minor, null, VersionPart.Build, null);

            var actualVersion = this.calculator.Calculate(this.faker.System.DirectoryPath());

            Assert.AreEqual(new Version(3, 2, 1), actualVersion);
        }

        /// <summary>
        /// Asserts that a mainline branch will calculate the version by incrementing the initial verison by each commit.
        /// </summary>
        [TestMethod]
        public void Calculate_MainlineBranchWithTag_ReturnsTagVersionIncrementedByEachCommitFromTag()
        {
            var solutionName = "pwr_Version_Core";
            this.SetupCommits(mainlineCommits: 1, nonMainlineCommits: 0, metadataCommits: 0);
            this.SetupBranch("master");
            this.SetupVersionIncrements(VersionPart.Minor);
            this.SetupSolutionManifest(new SolutionManifest(solutionName, new Version(0, 0, 0)));
            this.SetupVersionTag(new GitTag($"{solutionName}/5.0.0", this.faker.Random.Hash(7)));

            var actualVersion = this.calculator.Calculate(this.faker.System.DirectoryPath());

            Assert.AreEqual(new Version(5, 1, 0), actualVersion);
        }

        private void SetupMocksWithDefaults()
        {
            this.SetupSolutionRepoDefaults();
            this.SetupGitRepoDefaults();
            this.SetupPartCalculatorDefaults();
        }

        private void SetupGitRepoDefaults()
        {
            this.SetupBranch(this.faker.PickRandom(
                    "master",
                    $"release/{this.faker.Random.ReplaceNumbers("#.#")}",
                    $"feature/{this.faker.Random.Words().Replace(' ', '-')}"));
            this.SetupCommits(this.faker.Random.Number(1, 20), this.faker.Random.Number(1, 20), 0);
            this.SetupVersionTag(null);
            this.SetupBranch("master");
        }

        private void SetupVersionIncrements(params VersionPart?[] parts)
        {
            var setup = this.incrementCalculator
                .SetupSequence(p => p.Calculate(It.IsAny<GitCommit>(), It.IsAny<string>(), It.IsAny<bool>()));

            foreach (var part in parts)
            {
                setup = setup.Returns(part);
            }
        }

        private void SetupSolutionRepoDefaults()
        {
            this.SetupSolutionManifest(new SolutionManifest("pwr_Version_Core", new Version(0, 0)));
        }

        private List<GitCommit> SetupCommits(int mainlineCommits, int nonMainlineCommits, int metadataCommits)
        {
            if (mainlineCommits < 0)
            {
                throw new ArgumentException("You must provide a non-negative commit count.", nameof(mainlineCommits));
            }

            if (nonMainlineCommits < 0)
            {
                throw new ArgumentException("You must provide a non-negative commit count.", nameof(nonMainlineCommits));
            }

            var totalCommits = mainlineCommits + nonMainlineCommits;

            if (metadataCommits < 0 || totalCommits < metadataCommits)
            {
                throw new ArgumentException("You must a non-negative commit count that is not greater than the total number of commits.", nameof(metadataCommits));
            }

            var gitCommits = this.commitFaker.Generate(totalCommits);
            var metadataGitCommits = this.faker.PickRandom(gitCommits, metadataCommits);
            var nonMainlineGitCommits = gitCommits.Skip(mainlineCommits).Take(nonMainlineCommits);

            this.SetupCommitsToCalculate(gitCommits);
            this.SetupMetadataCommits(metadataGitCommits);
            this.SetupNonMainlineCommits(nonMainlineGitCommits);
            this.SetupHeadCommit(gitCommits.Last());

            return gitCommits;
        }

        private void SetupPartCalculatorDefaults()
        {
            this.SetupVersionIncrements();
        }

        private void SetupNonMainlineCommits(IEnumerable<GitCommit> gitCommits)
        {
            this.gitRepo.Setup(g => g.GetCommits(It.IsAny<string>(), It.IsNotIn("HEAD"), null))
                .Returns(gitCommits);
        }

        private void SetupCommitsToCalculate(IEnumerable<GitCommit> gitCommits)
        {
            this.gitRepo.Setup(g => g.GetCommits("HEAD", null))
                .Returns(gitCommits);

            this.gitRepo.Setup(g => g.GetCommits(It.IsAny<string>(), "HEAD", null))
                .Returns(gitCommits);
        }

        private void SetupMetadataCommits(IEnumerable<GitCommit> gitCommits)
        {
            this.gitRepo.Setup(g => g.GetCommits(It.IsAny<string>(), It.IsAny<string>(), It.IsNotNull<string>()))
                .Returns(gitCommits);
        }

        private void SetupSolutionManifest(SolutionManifest manifest)
        {
            this.solutionRepo
                .Setup(s => s.GetManifest(It.IsAny<string>()))
                .Returns(manifest);
        }

        private void SetupBranch(string branch)
        {
            this.gitRepo.Setup(g => g.GetBranch())
                .Returns(branch);
        }

        private void SetupVersionTag(GitTag tag)
        {
            this.gitRepo
                .Setup(g => g.GetTag("HEAD", It.IsAny<string>()))
                .Returns(tag);
        }

        private void SetupHeadCommit(GitCommit head)
        {
            this.gitRepo
                .Setup(g => g.GetCommit("HEAD"))
                .Returns(head);
        }
    }
}