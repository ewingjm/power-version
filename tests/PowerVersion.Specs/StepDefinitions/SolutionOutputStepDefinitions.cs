namespace PowerVersion.Specs.StepDefinitions
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using PowerVersion.Core.Enums;
    using PowerVersion.Core.Extensions;
    using PowerVersion.Specs.Extensions;
    using TechTalk.SpecFlow;

    /// <summary>
    /// Step bindings relating to the solution project build output.
    /// </summary>
    [Binding]
    public class SolutionOutputStepDefinitions
    {
        private const string XPathSolutionManifestVersion = "/ImportExportXml/SolutionManifest/Version";

        private readonly ScenarioContext scenarioCtx;
        private readonly TestContext testCtx;

        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionOutputStepDefinitions"/> class.
        /// </summary>
        /// <param name="scenarioCtx">The scenario context.</param>
        /// <param name="testCtx">The test context.</param>
        public SolutionOutputStepDefinitions(ScenarioContext scenarioCtx, TestContext testCtx)
        {
            this.scenarioCtx = scenarioCtx ?? throw new ArgumentNullException(nameof(scenarioCtx));
            this.testCtx = testCtx ?? throw new ArgumentNullException(nameof(testCtx));
        }

        /// <summary>
        /// Asserts that the outputted solution version has incremented the given version part of a known version.
        /// </summary>
        /// <param name="versionPart">The version part expected to have incremented.</param>
        [Then(@"the solution version is incremented to the next (major|minor|patch) version")]
        public void ThenTheSolutionVersionIsIncrementedToTheNextMajorVersion(string versionPart)
        {
            var knownVersion = this.scenarioCtx.GetKnownVersion();

            Version expectedVersion;
            switch (versionPart)
            {
                case "major":
                    expectedVersion = knownVersion.IncrementPart(VersionPart.Major);
                    break;
                case "minor":
                    expectedVersion = knownVersion.IncrementPart(VersionPart.Minor);
                    break;
                default:
                    expectedVersion = knownVersion.IncrementPart(VersionPart.Build);
                    break;
            }

            Assert.AreEqual(expectedVersion, this.GetBuildOutputSolutionVersion());
        }

        /// <summary>
        /// Asserts that the outputted solution version is the same as the version set by a tag.
        /// </summary>
        [Then(@"the solution version matches the version in the tag")]
        public void ThenTheSolutionVersionMatchesTheVersionInTheTag()
        {
            Assert.AreEqual(this.scenarioCtx.GetTaggedSolutionVersion(), this.GetBuildOutputSolutionVersion());
        }

        /// <summary>
        /// Asserts that the outputted solution version is the mainline version with a single increment equal to the highest increment of all of the feature branch commits.
        /// </summary>
        [Then(@"the version is the mainline version incremented by the highest version increment on the feature branch")]
        public void ThenTheVersionIsTheMainlineVersionIncrementedByTheHighestVersionIncrementOnTheFeatureBranch()
        {
            var mainlineVersion = this.scenarioCtx.GetKnownVersion();

            var featureBranch = this.scenarioCtx.GetFeatureBranchName();
            var featureBranchVersionParts = this.scenarioCtx.GetBranchCommitVersionParts(featureBranch);
            var actualVersion = this.GetBuildOutputSolutionVersion();

            Version expectedVersion = null;
            if (featureBranchVersionParts.Contains(VersionPart.Major))
            {
                expectedVersion = mainlineVersion.IncrementPart(VersionPart.Major);
            }
            else if (featureBranchVersionParts.Contains(VersionPart.Minor))
            {
                expectedVersion = mainlineVersion.IncrementPart(VersionPart.Minor);
            }
            else if (featureBranchVersionParts.Contains(VersionPart.Build))
            {
                expectedVersion = mainlineVersion.IncrementPart(VersionPart.Build);
            }

            Assert.AreEqual(expectedVersion, actualVersion);
        }

        /// <summary>
        /// Asserts that the outputted solution version is the mainline version with a revision number equal to the count of release branch commits that result in a solution version increment.
        /// </summary>
        [Then(@"the version is the mainline version with a revision number equal to the count of incrementing commits on the release branch")]
        public void ThenTheVersionIsTheMainlineVersionWithARevisionNumberEqualToTheCountOfIncrementingCommitsOnTheReleaseBranch()
        {
            var mainlineVersion = this.scenarioCtx.GetKnownVersion();

            var releaseBranch = this.scenarioCtx.GetReleaseBranchName();
            var releaseBranchVersionParts = this.scenarioCtx.GetBranchCommitVersionParts(releaseBranch);
            var actualVersion = this.GetBuildOutputSolutionVersion();

            var expectedVersion = mainlineVersion.IncrementPart(
                VersionPart.Revision,
                releaseBranchVersionParts.Count());

            Assert.AreEqual(expectedVersion, actualVersion);
        }

        private Version GetBuildOutputSolutionVersion()
        {
            var projectDirectory = this.scenarioCtx.GetProjectDirectory();
            var outputDirectory = Path.Combine(projectDirectory, "bin", "Debug");
            var archivePath = Path.Combine(outputDirectory, $"{this.scenarioCtx.GetSolutionName()}.zip");
            var extractDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            this.testCtx.WriteLine($"Extracting solution zip at {archivePath} to {extractDirectory}.");
            ZipFile.ExtractToDirectory(archivePath, extractDirectory);

            var solutionXml = Path.Combine(extractDirectory, "Solution.xml");
            this.testCtx.WriteLine($"Reading solution version from {solutionXml}.");

            var version = new Version(XDocument.Load(solutionXml).XPathSelectElement(XPathSolutionManifestVersion).Value);
            this.testCtx.WriteLine($"Current version is {version}.");
            return version;
        }
    }
}
