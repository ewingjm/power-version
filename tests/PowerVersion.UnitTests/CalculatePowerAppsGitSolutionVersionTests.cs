namespace PowerVersion.UnitTests
{
    using System;
    using System.IO;
    using System.IO.Abstractions;
    using Microsoft.Build.Framework;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using PowerVersion.Core.Enums;
    using PowerVersion.Core.Interfaces;
    using PowerVersion.MSBuild;

    /// <summary>
    /// Unit tests for the <see cref="CalculatePowerAppsGitSolutionVersion"/> task.
    /// </summary>
    [TestClass]
    public class CalculatePowerAppsGitSolutionVersionTests
    {
        private const string DefaultMetadataPath = "C://repos/repository/src/solution/metadata";

        private static readonly Version DefaultVersion = new Version(1, 2, 2);

        private readonly Mock<IBuildEngine> buildEngine;
        private readonly Mock<IFileSystem> fileSystem;
        private readonly Mock<ISolutionVersionCalculator> versionCalculator;
        private readonly Mock<ITaskItem> metadataPath;
        private readonly Mock<IDirectory> directory;

        private readonly CalculatePowerAppsGitSolutionVersion task;

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculatePowerAppsGitSolutionVersionTests"/> class.
        /// </summary>
        public CalculatePowerAppsGitSolutionVersionTests()
        {
            this.buildEngine = new Mock<IBuildEngine>();
            this.metadataPath = new Mock<ITaskItem>();
            this.fileSystem = new Mock<IFileSystem>();
            this.directory = new Mock<IDirectory>();
            this.versionCalculator = new Mock<ISolutionVersionCalculator>();

            this.SetupMocksWithDefaults();

            this.task = this.CreateTask();
        }

        /// <summary>
        /// Asserts that an exception is thrown when a file system is not provided to the constructor.
        /// </summary>
        [TestMethod]
        public void Constructor_FileSystemNotProvided_Throws()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                new CalculatePowerAppsGitSolutionVersion(null, this.versionCalculator.Object));
        }

        /// <summary>
        /// Asserts that an exception is thrown when a version calculator not provided to the constructor.
        /// </summary>
        [TestMethod]
        public void Constructor_VersionCalculatorNotProvided_Throws()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                new CalculatePowerAppsGitSolutionVersion(this.fileSystem.Object, null));
        }

        /// <summary>
        /// Asserts that an exception is thrown when a metadata path is not found.
        /// </summary>
        [TestMethod]
        public void Execute_MetadataPathNotProvided_Throws()
        {
            this.SetupMetadataPath(string.Empty);

            Assert.ThrowsException<ArgumentNullException>(() => this.task.Execute());
        }

        /// <summary>
        /// Asserts that an exception is thrown when a metadata path doesn't exist.
        /// </summary>
        [TestMethod]
        public void Execute_MetadataPathNotExists_Throws()
        {
            this.SetupMetadataPathExists(false);

            Assert.ThrowsException<DirectoryNotFoundException>(() => this.task.Execute());
        }

        /// <summary>
        /// Asserts that the version returned by the version calculator is assigned to <see cref="CalculatePowerAppsGitSolutionVersion.SolutionVersionFull"/>.
        /// </summary>
        [TestMethod]
        public void Execute_VersionReturnedByCalculator_AssignsVersionToOutputSolutionVersionFull()
        {
            var expectedVersion = new Version(1, 4, 0);
            this.SetupCalculatedVersion(expectedVersion);

            this.task.Execute();

            Assert.AreEqual(expectedVersion.ToString(), this.task.SolutionVersionFull);
        }

        /// <summary>
        /// Asserts that the last part updated of the version returned by the version calculator is assigned to <see cref="CalculatePowerAppsGitSolutionVersion.SolutionVersionPart"/>.
        /// </summary>
        /// <param name="version">The version returned by the version calculator.</param>
        /// <param name="expectedPart">The expected part.</param>
        [DataTestMethod]
        [DataRow("1.0.0.0", VersionPart.Major)]
        [DataRow("1.1.0.0", VersionPart.Minor)]
        [DataRow("1.1.1.0", VersionPart.Build)]
        [DataRow("1.1.1.1", VersionPart.Revision)]
        public void Execute_VersionReturnedByCalculator_AssignsLastUpdatedPartToOutputSolutionVersionPart(string version, VersionPart expectedPart)
        {
            this.SetupCalculatedVersion(new Version(version));

            this.task.Execute();

            Assert.AreEqual(expectedPart.ToString(), this.task.SolutionVersionPart);
        }

        /// <summary>
        /// Asserts that the version returned by the version calculator has the last updated part decremented and the result assigned to <see cref="CalculatePowerAppsGitSolutionVersion.SolutionVersion"/>.
        /// </summary>
        /// <param name="version">The version returned by the version calculator.</param>
        /// <param name="expectedOutput">The expected version output.</param>
        [DataTestMethod]
        [DataRow("1.0.0", "0.0.0")]
        [DataRow("1.1.0", "1.0.0")]
        [DataRow("1.1.1", "1.1.0")]
        [DataRow("1.1.1.1", "1.1.1.0")]
        public void Execute_VersionReturnedByCalculator_AssignsDecrementedVersionToOutputSolutionVersion(string version, string expectedOutput)
        {
            this.SetupCalculatedVersion(new Version(version));

            this.task.Execute();

            Assert.AreEqual(expectedOutput, this.task.SolutionVersion);
        }

        private CalculatePowerAppsGitSolutionVersion CreateTask()
        {
            return new CalculatePowerAppsGitSolutionVersion(this.fileSystem.Object, this.versionCalculator.Object)
            {
                BuildEngine = this.buildEngine.Object,
                MetadataPath = this.metadataPath.Object,
                MainlineBranch = "master",
                ReleaseBranchPrefix = "release/",
            };
        }

        private void SetupMocksWithDefaults()
        {
            this.SetupParameterDefaults();
            this.SetupFileSystemDefaults();
            this.SetupVersionCalculatorDefaults();
        }

        private void SetupParameterDefaults()
        {
            this.SetupMetadataPath();
        }

        private void SetupVersionCalculatorDefaults()
        {
            this.SetupCalculatedVersion();
        }

        private void SetupFileSystemDefaults()
        {
            this.SetupMetadataPathExists();
            this.SetupFileSystemDirectory();
        }

        private void SetupCalculatedVersion(Version version = null)
        {
            this.versionCalculator
                .Setup(v => v.Calculate(this.metadataPath.Object.ItemSpec, It.IsAny<string>(), It.IsAny<string>()))
                .Returns(version ?? DefaultVersion);
        }

        private void SetupFileSystemDirectory()
        {
            this.fileSystem.SetupGet(fs => fs.Directory)
                .Returns(this.directory.Object);
        }

        private void SetupMetadataPathExists(bool exists = true)
        {
            this.directory.Setup(d => d.Exists(It.IsAny<string>()))
                .Returns(exists);
        }

        private void SetupMetadataPath(string metadataPath = DefaultMetadataPath)
        {
            this.metadataPath.SetupGet(i => i.ItemSpec)
                .Returns(metadataPath);
        }
    }
}