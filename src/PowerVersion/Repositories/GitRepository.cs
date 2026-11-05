namespace PowerVersion.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text.RegularExpressions;
    using PowerVersion.Core.Interfaces;
    using PowerVersion.Core.Models;

    /// <summary>
    /// A Git repository.
    /// </summary>
    public class GitRepository : IGitRepository
    {
        private const char FieldSeparator = '\u001f';
        private const char RecordSeparator = '\u001e';

        // Control-character separators cannot appear in commit messages, so no escaping is required.
        private const string CommitFormat = "%h%x1f%s%x1f%b%x1e";

        private readonly string path;

        /// <summary>
        /// Initializes a new instance of the <see cref="GitRepository"/> class.
        /// </summary>
        public GitRepository()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GitRepository"/> class.
        /// </summary>
        /// <param name="path">The directoy to execute Git commands.</param>
        public GitRepository(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException($"'{nameof(path)}' cannot be null or empty.", nameof(path));
            }

            this.path = path;
        }

        /// <inheritdoc/>
        public string GetBranch()
        {
            return this.ExecuteCommand("symbolic-ref --short HEAD");
        }

        /// <inheritdoc/>
        public GitCommit GetCommit(string reference = "HEAD")
        {
            return ParseCommits(this.ExecuteCommand($"log -1 {reference} --pretty=format:{CommitFormat} --no-abbrev-commit")).Single();
        }

        /// <inheritdoc/>
        public IEnumerable<GitCommit> GetCommits(string reference = "HEAD", string path = null)
        {
            return ParseCommits(this.ExecuteCommand($"--no-pager log {reference} --pretty=format:{CommitFormat} --no-abbrev-commit --reverse {(path != null ? $"-- {path}" : string.Empty)}"));
        }

        /// <inheritdoc/>
        public IEnumerable<GitCommit> GetCommits(string left, string right, string path = null)
        {
            if (string.IsNullOrEmpty(left))
            {
                throw new ArgumentException($"'{nameof(left)}' cannot be null or empty.", nameof(left));
            }

            if (string.IsNullOrEmpty(right))
            {
                throw new ArgumentException($"'{nameof(right)}' cannot be null or empty.", nameof(right));
            }

            return ParseCommits(this.ExecuteCommand($"--no-pager log {left}..{right} --pretty=format:{CommitFormat} --no-abbrev-commit --reverse {(path != null ? $"-- {path}" : string.Empty)}"));
        }

        /// <inheritdoc/>
        public GitTag GetTag(string reference = "HEAD", string match = "*")
        {
            string tag;

            try
            {
                tag = this.ExecuteCommand($"describe --tags --abbrev=7 --match \"{match}\" {reference}");
            }
            catch (Exception ex) when (ex.Message.Contains("fatal: No names found, cannot describe anything."))
            {
                return null;
            }

            var regexMatch = Regex.Match(tag, @"^(?<tag>.+)-(?<additionalCommits>\d+)-g(?<commitHash>.+)$");

            var tagName = regexMatch.Success ? regexMatch.Groups["tag"].ToString() : tag;
            var tagCommit = this.ExecuteCommand($"rev-list -n 1 --abbrev-commit {tagName}");

            return new GitTag(tagName, tagCommit);
        }

        /// <inheritdoc/>
        public GitCommit GetMergeBase(string left, string right)
        {
            var mergeBase = this.ExecuteCommand($"merge-base {left} {right}");

            return ParseCommits(this.ExecuteCommand($"show --quiet --pretty=format:{CommitFormat} {mergeBase}")).Single();
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetBranchesWithTag(string tag)
        {
            return this.ExecuteCommand($"branch --contains tags/{tag}")
                .Split('\n')
                .Select(s => s.Substring(2));
        }

        private static IEnumerable<GitCommit> ParseCommits(string output)
        {
            return output
                .Split(new[] { RecordSeparator }, StringSplitOptions.RemoveEmptyEntries)
                .Select(record => record.TrimStart('\r', '\n'))
                .Where(record => record.Length > 0)
                .Select(record =>
                {
                    var fields = record.Split(new[] { FieldSeparator }, 3);

                    return new GitCommit(fields[0], fields[1], fields[2]);
                })
                .ToList();
        }

        private string ExecuteCommand(string command)
        {
            if (string.IsNullOrEmpty(command))
            {
                throw new ArgumentException($"'{nameof(command)}' cannot be null or empty.", nameof(command));
            }

            using (var process = new Process())
            {
                process.StartInfo.FileName = "git";
                process.StartInfo.Arguments = $"{command}";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.WorkingDirectory = this.path ?? string.Empty;

                process.Start();
                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    throw new Exception(error);
                }

                return output.TrimEnd('\n');
            }
        }
    }
}
