namespace PowerVersion
{
    using System.Text.RegularExpressions;
    using PowerVersion.Core.Enums;
    using PowerVersion.Core.Interfaces;
    using PowerVersion.Core.Models;

    /// <summary>
    /// Calculates the solution version part to increment for a commit.
    /// </summary>
    public class CommitIncrementCalculator : ICommitIncrementCalculator
    {
        private const string CommitSubjectMajorBumpRegex = "(\\w+)(\\([\\w\\s]*\\))?!:";
        private const string CommitSubjectMinorBumpRegex = "(feat)(\\([\\w\\s]*\\))?:";
        private const string CommitSubjectPatchBumpRegex = "(build|chore|ci|docs|fix|perf|refactor|revert|style|test)(\\([\\w\\s]*\\))?:";
        private const string CommitBodyMajorBumpRegexFormat = "\\+semver\\({0}\\):\\s?(breaking|major)";
        private const string CommitBodyMinorBumpRegexFormat = "\\+semver\\({0}\\):\\s?(feature|minor)";
        private const string CommitBodyPatchBumpRegexFormat = "\\+semver\\({0}\\):\\s?(fix|patch)";

        /// <inheritdoc/>
        public VersionPart? Calculate(GitCommit commit, string solution, bool solutionUpdate)
        {
            if (!solutionUpdate)
            {
                return this.GetCommitBodyBump(commit.Body, solution);
            }

            return this.GetCommitBodyBump(commit.Body, solution)
                ?? this.GetCommitSubjectBump(commit.Subject)
                ?? VersionPart.Build;
        }

        private VersionPart? GetCommitSubjectBump(string subject)
        {
            if (string.IsNullOrEmpty(subject))
            {
                return null;
            }

            if (Regex.IsMatch(subject, CommitSubjectMajorBumpRegex))
            {
                return VersionPart.Major;
            }
            else if (Regex.IsMatch(subject, CommitSubjectMinorBumpRegex))
            {
                return VersionPart.Minor;
            }
            else if (Regex.IsMatch(subject, CommitSubjectPatchBumpRegex))
            {
                return VersionPart.Build;
            }

            return null;
        }

        private VersionPart? GetCommitBodyBump(string body, string solution)
        {
            if (string.IsNullOrEmpty(body))
            {
                return null;
            }

            if (Regex.IsMatch(body, string.Format(CommitBodyMajorBumpRegexFormat, solution)))
            {
                return VersionPart.Major;
            }
            else if (Regex.IsMatch(body, string.Format(CommitBodyMinorBumpRegexFormat, solution)))
            {
                return VersionPart.Minor;
            }
            else if (Regex.IsMatch(body, string.Format(CommitBodyPatchBumpRegexFormat, solution)))
            {
                return VersionPart.Build;
            }

            return null;
        }
    }
}
