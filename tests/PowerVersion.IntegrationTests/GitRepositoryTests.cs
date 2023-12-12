namespace PowerVersion.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using Bogus;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using PowerVersion.Core.Models;
    using PowerVersion.Repositories;

    /// <summary>
    /// Integration tests for the <see cref="GitRepository"/> class.
    /// </summary>
    [TestClass]
    public class GitRepositoryTests
    {
        private readonly Faker faker;
        private readonly string repositoryPath;

        private readonly GitRepository gitRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="GitRepositoryTests"/> class.
        /// </summary>
        public GitRepositoryTests()
        {
            this.faker = new Faker();
            this.repositoryPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            this.gitRepository = new GitRepository(this.repositoryPath);

            this.CreateGitRepo();
        }

        /// <summary>
        /// deletes the initialised repository after test execution.
        /// </summary>
        [TestCleanup]
        public void DeleteRepository()
        {
            this.NormalizeRepositoryAttributes(this.repositoryPath);

            Directory.Delete(Path.Combine(this.repositoryPath), true);
        }

        /// <summary>
        /// Asserts that <see cref="GitRepository.GetBranch"/> returns the current checked out branch name.
        /// </summary>
        [TestMethod]
        public void GetBranch_BranchCheckedOut_ReturnsCheckedOutBranch()
        {
            var expectedBranch = this.faker.Random.Words().ToLower().Replace(" ", "-");
            this.ExecuteGitCommand($"checkout -b {expectedBranch}");

            var actualBranch = this.gitRepository.GetBranch();

            Assert.AreEqual(expectedBranch, actualBranch);
        }

        /// <summary>
        /// Asserts that <see cref="GitRepository.GetCommit(string)"/> with no ref will return the head commit.
        /// </summary>
        [TestMethod]
        public void GetCommit_NoRefProvided_ReturnsHeadCommit()
        {
            var expectedCommit = this.CreateGitCommit();

            var actualCommit = this.gitRepository.GetCommit();

            Assert.AreEqual(expectedCommit, actualCommit);
        }

        /// <summary>
        /// Asserts that <see cref="GitRepository.GetCommit(string)"/> with a ref specified will return the commit found by the ref.
        /// </summary>
        [TestMethod]
        public void GetCommit_RefProvided_ReturnsCommitFromRef()
        {
            var expectedCommit = this.CreateGitCommit();
            this.CreateGitCommit();

            var actualCommit = this.gitRepository.GetCommit("HEAD^");

            Assert.AreEqual(expectedCommit, actualCommit);
        }

        /// <summary>
        /// Asserts that <see cref="GitRepository.GetCommit(string)"/> with a ref specified will throw an exception if the ref is not found.
        /// </summary>
        [TestMethod]
        public void GetCommit_RefNotValid_Throws()
        {
            this.CreateGitCommit();

            Assert.ThrowsException<Exception>(() => this.gitRepository.GetCommit(this.faker.Random.Word()));
        }

        /// <summary>
        /// Asserts that <see cref="GitRepository.GetCommits(string, string)"/> with no parameters specified will return all commits to the HEAD.
        /// </summary>
        [TestMethod]
        public void GetCommits_NoParametersProvided_ReturnsAllCommitsToHead()
        {
            var expectedCommits = this.CreateMultipleCommits(2, 5);

            var actualCommits = this.gitRepository.GetCommits();

            CollectionAssert.AreEqual(expectedCommits, actualCommits.ToArray());
        }

        /// <summary>
        /// Asserts that <see cref="GitRepository.GetCommits(string, string)"/> with a reference parameter specified will return all commits to the provided Git ref.
        /// </summary>
        [TestMethod]
        public void GetCommits_ReferenceProvided_ReturnsAllCommitsToReference()
        {
            var commits = this.CreateMultipleCommits(2, 5);
            var toCommit = this.faker.PickRandom(commits);
            var expectedCommits = commits.Take(commits.IndexOf(toCommit) + 1).ToArray();

            var actualCommits = this.gitRepository.GetCommits(toCommit.Hash);

            CollectionAssert.AreEqual(expectedCommits, actualCommits.ToArray());
        }

        /// <summary>
        /// Asserts that <see cref="GitRepository.GetCommits(string, string)"/> with a path parameter specified will return all commits with changes under that path.
        /// </summary>
        [TestMethod]
        public void GetCommits_PathProvided_ReturnsAllCommitsWithChangesUnderPath()
        {
            var path = Path.Combine(this.repositoryPath, "src", "solutions", "pwr_Version_Core");
            this.CreateMultipleCommits(1, 2);
            var metadataCommit = this.CreateGitCommit(path: path);
            var expectedCommits = new[] { metadataCommit };

            var actualCommits = this.gitRepository.GetCommits(path: path);

            CollectionAssert.AreEqual(expectedCommits, actualCommits.ToArray());
        }

        /// <summary>
        /// Asserts that <see cref="GitRepository.GetCommits(string, string, string)"/> with left and right parameters specified will return all commits in right that are not in left.
        /// </summary>
        [TestMethod]
        public void GetCommits_LeftAndRightProvided_ReturnsAllCommitsInRightMissingFromLeft()
        {
            var commits = this.CreateMultipleCommits(3, 5);
            var leftCommit = commits.First();
            var rightCommit = commits.Last();
            var expectedCommits = commits.Skip(1).ToArray();

            var actualCommits = this.gitRepository.GetCommits(left: leftCommit.Hash, right: rightCommit.Hash);

            CollectionAssert.AreEqual(expectedCommits, actualCommits.ToArray());
        }

        /// <summary>
        /// Asserts that <see cref="GitRepository.GetCommits(string, string, string)"/> with left and right parameters specified alongside a path parameter will return all commits in right that are not in left that have changes under the path.
        /// </summary>
        [TestMethod]
        public void GetCommits_LeftRightAndPathProvided_ReturnsAllCommitsInRightMissingFromLeftWithUpdatesUnderPath()
        {
            var leftCommit = this.CreateGitCommit();
            var path = Path.Combine(this.repositoryPath, "src", "solutions", "pwr_Version_Core");
            var metadataCommit = this.CreateGitCommit(path: path);
            var expectedCommits = new[] { metadataCommit };
            var rightCommit = this.CreateMultipleCommits(2, 5).ElementAt(0);

            var actualCommits = this.gitRepository.GetCommits(leftCommit.Hash, rightCommit.Hash, path);

            CollectionAssert.AreEqual(expectedCommits, actualCommits.ToArray());
        }

        /// <summary>
        /// Asserts that <see cref="GitRepository.GetBranchesWithTag(string)"/> returns a collection of branches that contain a given tag.
        /// </summary>
        [TestMethod]
        public void GetBranchesWithTag_BranchesContainTag_ReturnsBranchesWithTag()
        {
            var branch = this.SwitchToGitBranch();
            this.CreateGitCommit();
            var tag = this.CreateGitTag();
            var expectedBranches = new[] { branch };

            var actualBranches = this.gitRepository.GetBranchesWithTag(tag);

            CollectionAssert.AreEqual(expectedBranches, actualBranches.ToArray());
        }

        /// <summary>
        /// Asserts that <see cref="GitRepository.GetMergeBase(string, string)"/> returns the common ancestor commit of two refs.
        /// </summary>
        [TestMethod]
        public void GetMergeBase_RefsShareCommonAncestor_ReturnsMergeBase()
        {
            var left = this.SwitchToGitBranch();
            var expectedMergeBase = this.CreateGitCommit();
            var right = this.SwitchToGitBranch();
            this.CreateGitCommit();

            var actualMergeBase = this.gitRepository.GetMergeBase(left, right);

            Assert.AreEqual(expectedMergeBase, actualMergeBase);
        }

        /// <summary>
        /// Asserts that <see cref="GitRepository.GetTag(string, string)"/> returns only tags reachable by the provided ref.
        /// </summary>
        [TestMethod]
        public void GetTag_RefProvided_ReturnsMostRecentTagFromReference()
        {
            var referenceCommit = this.CreateGitCommit();
            var expectedTag = this.CreateGitTag("pwr_Version_Core/v1.0.0");
            this.CreateGitCommit();
            this.CreateGitTag("pwr_Version_App/v1.0.0");

            var actualTag = this.gitRepository.GetTag(referenceCommit.Hash);

            Assert.AreEqual(expectedTag, actualTag.Name);
        }

        /// <summary>
        /// Asserts that <see cref="GitRepository.GetTag(string, string)"/> filters tags using the provided pattern (if provided).
        /// </summary>
        [TestMethod]
        public void GetTag_PatternProvided_ReturnsMostRecentTagMatchingPattern()
        {
            this.CreateGitCommit();
            var expectedTag = this.CreateGitTag("pwr_Version_Core/v1.0.0");
            this.CreateGitCommit();
            this.CreateGitTag("pwr_Version_App/v1.0.0");

            var actualTag = this.gitRepository.GetTag(match: "pwr_Version_Core/v1.0.0");

            Assert.AreEqual(expectedTag, actualTag.Name);
        }

        private string CreateGitTag(string tag = null)
        {
            tag = tag ?? this.faker.Random.AlphaNumeric(5);

            this.ExecuteGitCommand($"tag {tag}");

            return tag;
        }

        private string SwitchToGitBranch(string branch = null)
        {
            branch = branch ?? this.faker.Random.Words().ToLower().Replace(' ', '-');

            this.ExecuteGitCommand($"switch -c {branch}");

            return branch;
        }

        private List<GitCommit> CreateMultipleCommits(int min, int max)
        {
            return Enumerable.Range(0, this.faker.Random.Number(min, max))
                .Select(i => this.CreateGitCommit())
                .ToList();
        }

        private void CreateGitRepo()
        {
            Directory.CreateDirectory(this.repositoryPath);

            this.ExecuteGitCommand("init");
        }

        private GitCommit CreateGitCommit(string subject = null, string path = ".")
        {
            var tempFile = Path.GetTempFileName();
            var fileDirectory = Path.Combine(this.repositoryPath, path);
            if (!Directory.Exists(fileDirectory))
            {
                Directory.CreateDirectory(fileDirectory);
            }

            File.Move(tempFile, Path.Combine(fileDirectory, Path.GetFileName(tempFile)));

            subject = subject ?? this.faker.Lorem.Sentence();

            this.ExecuteGitCommand("add .");
            this.ExecuteGitCommand($"commit -m \"{subject}\"");

            return new GitCommit(this.ExecuteGitCommand("rev-parse --short HEAD"), subject, null);
        }

        private string ExecuteGitCommand(string command)
        {
            if (string.IsNullOrEmpty(command))
            {
                throw new ArgumentException($"'{nameof(command)}' cannot be null or empty.", nameof(command));
            }

            using (var process = new Process())
            {
                process.StartInfo.FileName = "git";
                process.StartInfo.Arguments = command;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.WorkingDirectory = this.repositoryPath;

                process.Start();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    throw new Exception(process.StandardError.ReadToEnd());
                }

                return process.StandardOutput.ReadToEnd().TrimEnd('\n');
            }
        }

        private void NormalizeRepositoryAttributes(string path)
        {
            var files = Directory.GetFiles(path);
            var directories = Directory.GetDirectories(path);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
            }

            foreach (string directory in directories)
            {
                this.NormalizeRepositoryAttributes(directory);
            }

            File.SetAttributes(path, FileAttributes.Normal);
        }
    }
}
