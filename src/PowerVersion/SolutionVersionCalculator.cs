namespace PowerVersion
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using PowerVersion.Core.Enums;
    using PowerVersion.Core.Extensions;
    using PowerVersion.Core.Interfaces;
    using PowerVersion.Core.Models;

    /// <summary>
    /// Calculates a version from Git.
    /// </summary>
    public class SolutionVersionCalculator : ISolutionVersionCalculator
    {
        private readonly ISolutionRepository solutionRepo;
        private readonly IGitRepository gitRepo;
        private readonly ICommitIncrementCalculator partCalculator;

        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionVersionCalculator"/> class.
        /// </summary>
        /// <param name="solutionRepo">The solution repository.</param>
        /// <param name="gitRepo">The Git repository.</param>
        /// <param name="partCalculator">The version part calculator.</param>
        public SolutionVersionCalculator(ISolutionRepository solutionRepo, IGitRepository gitRepo, ICommitIncrementCalculator partCalculator)
        {
            this.solutionRepo = solutionRepo ?? throw new ArgumentNullException(nameof(solutionRepo));
            this.gitRepo = gitRepo ?? throw new ArgumentNullException(nameof(gitRepo));
            this.partCalculator = partCalculator ?? throw new ArgumentNullException(nameof(partCalculator));
        }

        /// <inheritdoc/>
        public Version Calculate(string metadataDirectory, string mainlineBranch = "master", string releaseBranchPrefix = "release/")
        {
            var manifest = this.solutionRepo.GetManifest(metadataDirectory);
            var versionTag = this.GetSolutionVersionTag(manifest.UniqueName);

            if (versionTag?.Hash == this.gitRepo.GetCommit().Hash)
            {
                return versionTag.Version;
            }

            var branch = this.gitRepo.GetBranch();
            var branchType = this.GetBranchType(branch, mainlineBranch, releaseBranchPrefix);
            var initialVersion = versionTag?.Version ?? new Version(manifest.Version);

            switch (branchType)
            {
                case BranchType.Mainline:
                    return this.CalculateMainlineBranchVersion(metadataDirectory, mainlineBranch, manifest.UniqueName, versionTag?.ToString(), initialVersion);
                case BranchType.Release:
                    return this.CalculateReleaseBranchVersion(metadataDirectory, branch, mainlineBranch, manifest.UniqueName, versionTag?.ToString(), initialVersion);
                default:
                    return this.CalculateFeatureBranchVersion(metadataDirectory, branch, mainlineBranch, manifest.UniqueName, versionTag?.ToString(), initialVersion);
            }
        }

        private Version CalculateMainlineBranchVersion(string metadataDirectory, string mainlineBranch, string solution, string versionTag = null, Version initialVersion = null)
        {
            var commitsToCalculate = this.GetCommitsToCalculate(versionTag);
            var metadataCommits = this.GetMetadataCommits(metadataDirectory, mainlineBranch, versionTag);

            return this.CalculateMainlineVersion(commitsToCalculate, metadataCommits, solution, initialVersion);
        }

        private Version CalculateReleaseBranchVersion(string metadataDirectory, string branch, string mainlineBranch, string solution, string versionTag = null, Version initialVersion = null)
        {
            var commitsToCalculate = this.GetCommitsToCalculate(versionTag);
            var metadataCommits = this.GetMetadataCommits(metadataDirectory, branch, versionTag);
            var releaseBranchCommits = this.GetNonMainlineCommits(branch, mainlineBranch, versionTag);

            var version = this.CalculateMainlineVersion(commitsToCalculate.Except(releaseBranchCommits), metadataCommits, solution, initialVersion);

            var releaseBranchIncrements = releaseBranchCommits
                .Select(r => this.partCalculator.Calculate(r, solution, metadataCommits.Contains(r)))
                .Where(p => p != null);

            return version.IncrementPart(VersionPart.Revision, releaseBranchIncrements.Count());
        }

        private Version CalculateFeatureBranchVersion(string metadataDirectory, string branch, string mainlineBranch, string solution, string versionTag = null, Version initialVersion = null)
        {
            var commitsToCalculate = this.GetCommitsToCalculate(versionTag);
            var metadataCommits = this.GetMetadataCommits(metadataDirectory, branch, versionTag);
            var featureBranchCommits = this.GetNonMainlineCommits(branch, mainlineBranch, versionTag);

            var version = this.CalculateMainlineVersion(commitsToCalculate.Except(featureBranchCommits), metadataCommits, solution, initialVersion);

            return version.IncrementPart(this.GetHighestVersionPart(featureBranchCommits, metadataCommits, solution));
        }

        private Version CalculateMainlineVersion(IEnumerable<GitCommit> mainlineCommits, IEnumerable<GitCommit> metadataCommits, string solution, Version initialVersion = null)
        {
            return mainlineCommits.Aggregate(initialVersion, (previousVersion, currentCommit) =>
            {
                if (this.TryGetCommitPart(currentCommit, metadataCommits, solution, out var versionPart))
                {
                    return previousVersion.IncrementPart(versionPart.Value);
                }

                return previousVersion;
            });
        }

        private IEnumerable<GitCommit> GetMetadataCommits(string metadataDirectory, string branch, string versionTag = null)
        {
            if (string.IsNullOrEmpty(versionTag))
            {
                return this.gitRepo.GetCommits(branch, metadataDirectory);
            }

            return this.gitRepo.GetCommits(left: versionTag, right: branch, path: metadataDirectory);
        }

        private IEnumerable<GitCommit> GetCommitsToCalculate(string versionTag = null)
        {
            if (string.IsNullOrEmpty(versionTag))
            {
                return this.gitRepo.GetCommits();
            }

            return this.gitRepo.GetCommits(left: versionTag, right: "HEAD");
        }

        private bool TryGetCommitPart(GitCommit commit, IEnumerable<GitCommit> metadataCommits, string solution, out VersionPart? versionPart)
        {
            versionPart = this.partCalculator.Calculate(commit, solution, metadataCommits.Contains(commit));

            return versionPart != null;
        }

        private IEnumerable<GitCommit> GetNonMainlineCommits(string branch, string mainlineBranch, string versionTag = null)
        {
            if (!string.IsNullOrEmpty(versionTag) && !this.gitRepo.GetBranchesWithTag(versionTag).Contains(mainlineBranch))
            {
                // Tag doesn't exist on master. Get release branch commits starting from tag rather than starting from master.
                return this.gitRepo.GetCommits(left: versionTag, right: branch);
            }

            return this.gitRepo.GetCommits(left: mainlineBranch, right: branch);
        }

        private VersionPart GetHighestVersionPart(IEnumerable<GitCommit> commits, IEnumerable<GitCommit> metadataCommits, string solution)
        {
            return commits.Aggregate(VersionPart.Build, (currentHighestVersionPart, commit) =>
            {
                if (currentHighestVersionPart == VersionPart.Major)
                {
                    // A commit incrementing major has already been found. Don't need to check the remaining commits.
                    return currentHighestVersionPart;
                }

                if (this.TryGetCommitPart(commit, metadataCommits, solution, out var versionPart))
                {
                    return versionPart.Value > currentHighestVersionPart ? versionPart.Value : currentHighestVersionPart;
                }

                return currentHighestVersionPart;
            });
        }

        private SolutionVersionTag GetSolutionVersionTag(string solution)
        {
            var tag = this.gitRepo.GetTag(match: $"{solution}/*");

            return tag != null ? new SolutionVersionTag(tag) : null;
        }

        private BranchType GetBranchType(string branch, string mainlineBranch, string releaseBranchPrefix)
        {
            if (branch == mainlineBranch)
            {
                return BranchType.Mainline;
            }
            else if (branch.StartsWith(releaseBranchPrefix))
            {
                return BranchType.Release;
            }

            return BranchType.Feature;
        }
    }
}
