namespace PowerVersion.MSBuild
{
    using System;
    using System.IO;
    using System.IO.Abstractions;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using PowerVersion.Core.Enums;
    using PowerVersion.Core.Extensions;
    using PowerVersion.Core.Interfaces;
    using PowerVersion.Repositories;

    /// <summary>
    /// A task to gets the version for a Power Apps solution using Git.
    /// </summary>
    public class CalculatePowerAppsGitSolutionVersion : Task
    {
        private readonly IFileSystem fileSystem;
        private readonly ISolutionVersionCalculator versionCalculator;

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculatePowerAppsGitSolutionVersion"/> class.
        /// </summary>
        public CalculatePowerAppsGitSolutionVersion()
        {
            this.fileSystem = new FileSystem();
            this.versionCalculator = new SolutionVersionCalculator(
                new SolutionRepository(),
                new GitRepository(),
                new CommitIncrementCalculator());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculatePowerAppsGitSolutionVersion"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="versionCalculator">The version calculator.</param>
        public CalculatePowerAppsGitSolutionVersion(IFileSystem fileSystem, ISolutionVersionCalculator versionCalculator)
            : base()
        {
            this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            this.versionCalculator = versionCalculator ?? throw new ArgumentNullException(nameof(versionCalculator));
        }

        /// <summary>
        /// Gets or sets the directory containing the solution metadata.
        /// </summary>
        [Required]
        public ITaskItem MetadataPath { get; set; }

        /// <summary>
        /// Gets or sets the name of the mainline branch.
        /// </summary>
        [Required]
        public string MainlineBranch { get; set; }

        /// <summary>
        /// Gets or sets the prefix of release branches (e.g. branches that may be used for hotfixing).
        /// </summary>
        [Required]
        public string ReleaseBranchPrefix { get; set; }

        /// <summary>
        /// Gets the solution version to be combined with the solution version part. This property is used by the Microsoft targets.
        /// </summary>
        [Output]
        public string SolutionVersion { get; private set; }

        /// <summary>
        /// Gets the solution version part to be combined with the solution version. This property is used by the Microsoft targets.
        /// </summary>
        [Output]
        public string SolutionVersionPart { get; private set; }

        /// <summary>
        /// Gets the complete solution version.
        /// </summary>
        [Output]
        public string SolutionVersionFull { get; private set; }

        /// <inheritdoc/>
        public override bool Execute()
        {
            this.ValidateParameters();

            try
            {
                var solutionVersionFull = this.GetSolutionVersion();
                var solutionVersionPart = solutionVersionFull.GetLastIncrementedPartOrDefault();
                var solutionVersion = solutionVersionFull.DecrementPart(solutionVersionPart);

                this.SetOutputs(solutionVersion, solutionVersionPart, solutionVersionFull);
            }
            catch (Exception ex)
            {
                this.Log.LogErrorFromException(ex);

                return false;
            }

            return true;
        }

        private void SetOutputs(Version solutionVersion, VersionPart solutionVersionPart, Version fullSolutionVersion)
        {
            this.SolutionVersion = solutionVersion.ToString();
            this.SolutionVersionPart = solutionVersionPart.ToString();
            this.SolutionVersionFull = fullSolutionVersion.ToString();
        }

        private Version GetSolutionVersion()
        {
            return this.versionCalculator.Calculate(
                this.MetadataPath.ItemSpec,
                this.MainlineBranch,
                this.ReleaseBranchPrefix);
        }

        private void ValidateParameters()
        {
            this.ValidateMetadataParameters();
            this.ValidateRepositoryParameters();
        }

        private void ValidateMetadataParameters()
        {
            if (string.IsNullOrEmpty(this.MetadataPath.ItemSpec))
            {
                throw new ArgumentNullException(nameof(this.MetadataPath));
            }

            if (!this.fileSystem.Directory.Exists(this.MetadataPath.ItemSpec))
            {
                throw new DirectoryNotFoundException($"Unable to find solution metadata directory at {this.MetadataPath.ItemSpec}");
            }
        }

        private void ValidateRepositoryParameters()
        {
            if (string.IsNullOrEmpty(this.MainlineBranch))
            {
                throw new ArgumentException($"You must provide a value for the {nameof(this.MainlineBranch)} parameter.");
            }
        }
    }
}
