namespace PowerVersion.UnitTests
{
    using Bogus;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using PowerVersion.Core.Enums;
    using PowerVersion.Core.Models;

    /// <summary>
    /// Unit tests for the <see cref="CommitIncrementCalculator"/> class.
    /// </summary>
    [TestClass]
    public class CommitIncrementCalculatorTests
    {
        private const string DefaultSolutionName = "pwr_Version_Core";

        private readonly CommitIncrementCalculator calculator;
        private readonly Faker faker;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommitIncrementCalculatorTests"/> class.
        /// </summary>
        public CommitIncrementCalculatorTests()
        {
            this.calculator = new CommitIncrementCalculator();
            this.faker = new Faker();
        }

        /// <summary>
        /// Asserts that a metadata commit with a subject prefix is incremented based on the prefix.
        /// </summary>
        /// <param name="subjectPrefix">The commit subject prefix.</param>
        /// <param name="expectedPart">The expected part to increment.</param>
        [DataTestMethod]
        [DataRow("build!:", VersionPart.Major)]
        [DataRow("chore!:", VersionPart.Major)]
        [DataRow("ci!:", VersionPart.Major)]
        [DataRow("docs!:", VersionPart.Major)]
        [DataRow("feat!:", VersionPart.Major)]
        [DataRow("fix!:", VersionPart.Major)]
        [DataRow("perf!:", VersionPart.Major)]
        [DataRow("refactor!:", VersionPart.Major)]
        [DataRow("revert!:", VersionPart.Major)]
        [DataRow("style!:", VersionPart.Major)]
        [DataRow("test!:", VersionPart.Major)]
        [DataRow("feat:", VersionPart.Minor)]
        [DataRow("build:", VersionPart.Build)]
        [DataRow("chore:", VersionPart.Build)]
        [DataRow("ci:", VersionPart.Build)]
        [DataRow("docs:", VersionPart.Build)]
        [DataRow("fix:", VersionPart.Build)]
        [DataRow("perf:", VersionPart.Build)]
        [DataRow("refactor:", VersionPart.Build)]
        [DataRow("revert:", VersionPart.Build)]
        [DataRow("style:", VersionPart.Build)]
        [DataRow("test:", VersionPart.Build)]
        public void Calculate_MetadataCommitWithPrefix_ReturnsPartDeterminedByCommitPrefix(string subjectPrefix, VersionPart expectedPart)
        {
            var commit = this.CreateCommit(subject: $"{subjectPrefix} {this.faker.Lorem.Sentence()}");

            var actualPart = this.calculator.Calculate(commit, DefaultSolutionName, true);

            Assert.AreEqual(expectedPart, actualPart);
        }

        /// <summary>
        /// Asserts that a metadata commit with a manual bump is incremented based on the manual bump.
        /// </summary>
        /// <param name="manualBump">The commit body manual bump.</param>
        /// <param name="expectedPart">The expected part.</param>
        [DataTestMethod]
        [DataRow("+semver(pwr_Version_Core): breaking", VersionPart.Major)]
        [DataRow("+semver(pwr_Version_Core): major", VersionPart.Major)]
        [DataRow("+semver(pwr_Version_Core): feature", VersionPart.Minor)]
        [DataRow("+semver(pwr_Version_Core): minor", VersionPart.Minor)]
        [DataRow("+semver(pwr_Version_Core): fix", VersionPart.Build)]
        [DataRow("+semver(pwr_Version_Core): patch", VersionPart.Build)]
        public void Calculate_MetadataCommitWithManualBump_ReturnsPartSpecifiedByManualBump(string manualBump, VersionPart expectedPart)
        {
            var commit = this.CreateCommit(body: $"{this.faker.Lorem.Sentences()}\n{manualBump}");

            var actualPart = this.calculator.Calculate(commit, DefaultSolutionName, true);

            Assert.AreEqual(expectedPart, actualPart);
        }

        /// <summary>
        /// Asserts that a non-metadata commit without a manual bump returns null.
        /// </summary>
        [TestMethod]
        public void Calculate_NonMetadataCommitWithoutManualBump_ReturnsNull()
        {
            var commit = this.CreateCommit(subject: $"fix: {this.faker.Lorem.Sentence()}");

            var actualPart = this.calculator.Calculate(commit, DefaultSolutionName, false);

            Assert.IsNull(actualPart);
        }

        /// <summary>
        /// Asserts that a non-metadata commit without a manual bump returns null.
        /// </summary>
        [TestMethod]
        public void Calculate_NonMetadataCommitWithManualBump_ReturnsPartSpecifiedByManualBump()
        {
            var commit = this.CreateCommit(body: $"{this.faker.Lorem.Sentences()}\n+semver(pwr_Version_Core): breaking");

            var actualPart = this.calculator.Calculate(commit, DefaultSolutionName, false);

            Assert.AreEqual(VersionPart.Major, actualPart);
        }

        /// <summary>
        /// Asserts that a non-metadata commit without a manual bump returns null.
        /// </summary>
        [TestMethod]
        public void Calculate_MetadataCommitWithoutPrefixOrManualBump_ReturnsBuildAsADefault()
        {
            var commit = this.CreateCommit(subject: "no conventional commit prefix");

            var actualPart = this.calculator.Calculate(commit, DefaultSolutionName, true);

            Assert.AreEqual(VersionPart.Build, actualPart);
        }

        private GitCommit CreateCommit(string hash = null, string subject = null, string body = null)
        {
            return new GitCommit(
                hash ?? this.faker.Random.Hash(7),
                subject ?? $"{this.faker.PickRandom("fix", "feat")}: {this.faker.Lorem.Sentence()}",
                body ?? string.Empty);
        }
    }
}
