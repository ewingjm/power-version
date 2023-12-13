namespace PowerVersion.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text.Json;
    using System.Text.RegularExpressions;
    using PowerVersion.Core.Interfaces;
    using PowerVersion.Core.Models;

    /// <summary>
    /// A Git repository.
    /// </summary>
    public class GitRepository : IGitRepository
    {
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
            return JsonSerializer.Deserialize<GitCommit>(
                this.ExecuteJsonCommand($"log -1 {reference} --pretty=format:{{\\\"Hash\\\":\\\"%h\\\",\\\"Subject\\\":\\\"%s\\\",\\\"Body\\\":\\\"%b\\\"}} --no-abbrev-commit").Replace("\n", "\\n"));
        }

        /// <inheritdoc/>
        public IEnumerable<GitCommit> GetCommits(string reference = "HEAD", string path = null)
        {
            return JsonSerializer.Deserialize<GitCommit[]>(
                this.ExecuteArrayCommand($"log {reference} --pretty=format:{{\\\"Hash\\\":\\\"%h\\\",\\\"Subject\\\":\\\"%s\\\",\\\"Body\\\":\\\"%b\\\"}} --no-abbrev-commit --reverse {(path != null ? $"-- {path}" : string.Empty)}"));
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

            return JsonSerializer.Deserialize<GitCommit[]>(
                this.ExecuteArrayCommand($"log {left}..{right} --pretty=format:{{\\\"Hash\\\":\\\"%h\\\",\\\"Subject\\\":\\\"%s\\\",\\\"Body\\\":\\\"%b\\\"}} --no-abbrev-commit --reverse {(path != null ? $"-- {path}" : string.Empty)}"));
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

            return JsonSerializer.Deserialize<GitCommit>(
                this.ExecuteJsonCommand($"show --quiet --pretty=format:{{\\\"Hash\\\":\\\"%h\\\",\\\"Subject\\\":\\\"%s\\\",\\\"Body\\\":\\\"%b\\\"}} {mergeBase}"));
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetBranchesWithTag(string tag)
        {
            return this.ExecuteCommand($"branch --contains tags/{tag}")
                .Split('\n')
                .Select(s => s.Substring(2));
        }

        private string ExecuteArrayCommand(string command)
        {
            return $"[{Regex.Replace(this.ExecuteJsonCommand(command), "}\\\\n{", "},\n{")}]";
        }

        private string ExecuteJsonCommand(string command)
        {
            return this.ExecuteCommand(command).Replace("\n", "\\n");
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
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    throw new Exception(process.StandardError.ReadToEnd());
                }

                return process.StandardOutput.ReadToEnd().TrimEnd('\n');
            }
        }
    }
}
