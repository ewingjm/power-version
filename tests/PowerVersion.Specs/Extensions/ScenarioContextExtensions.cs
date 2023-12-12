namespace PowerVersion.Specs.Extensions
{
    using System;
    using System.Collections.Generic;
    using PowerVersion.Core.Enums;
    using PowerVersion.Specs.StepDefinitions;
    using TechTalk.SpecFlow;

    /// <summary>
    /// Extensions to the <see cref="ScenarioContext"/> class.
    /// </summary>
    internal static class ScenarioContextExtensions
    {
        /// <summary>
        /// Gets the feature branch name created by <see cref="GitStepDefinitions.GivenIHaveCheckedOutAFeatureBranch"/>.
        /// </summary>
        /// <param name="scenarioCtx">The scenario context.</param>
        /// <returns>The name of the feature branch.</returns>
        /// <exception cref="Exception">Thrown if not added to the context.</exception>
        internal static string GetFeatureBranchName(this ScenarioContext scenarioCtx)
        {
            return scenarioCtx.Get<string>(
                GitStepDefinitions.ContextVariableFeatureBranchName,
                $"A feature branch has not been created with the {nameof(GitStepDefinitions.GivenIHaveCheckedOutAFeatureBranch)} binding.");
        }

        /// <summary>
        /// Gets the release branch name created by <see cref="GitStepDefinitions.GivenIHaveCheckedOutAReleaseBranch"/>.
        /// </summary>
        /// <param name="scenarioCtx">The scenario context.</param>
        /// <returns>The name of the release branch.</returns>
        /// <exception cref="Exception">Thrown if not added to the context.</exception>
        internal static string GetReleaseBranchName(this ScenarioContext scenarioCtx)
        {
            return scenarioCtx.Get<string>(
                GitStepDefinitions.ContextVariableReleaseBranchName,
                $"A release branch has not been created with the {nameof(GitStepDefinitions.GivenIHaveCheckedOutAReleaseBranch)} binding.");
        }

        /// <summary>
        /// Gets the repository directory created by <see cref="GitStepDefinitions.GivenAGitRepositoryHasBeenInitalised"/>.
        /// </summary>
        /// <param name="scenarioCtx">The scenario context.</param>
        /// <returns>The path to the repository directory.</returns>
        /// <exception cref="Exception">Thrown if not added to the context.</exception>
        internal static string GetRepositoryDirectory(this ScenarioContext scenarioCtx)
        {
            return scenarioCtx.Get<string>(
                GitStepDefinitions.ContextVariableRepoDirectory,
                $"A repository has not been initialised with the {nameof(GitStepDefinitions.GivenAGitRepositoryHasBeenInitalised)} binding.");
        }

        /// <summary>
        /// Gets the name of the solution created by <see cref="PacStepDefinitions.GivenASolutionProjectHasBeenCreatedWithThePowerAppsCLI"/>.
        /// </summary>
        /// <param name="scenarioCtx">The scenario context.</param>
        /// <returns>The name of the solution.</returns>
        /// <exception cref="Exception">Thrown if not added to the context.</exception>
        internal static string GetSolutionName(this ScenarioContext scenarioCtx)
        {
            return scenarioCtx.Get<string>(
                PacStepDefinitions.ContextVariableSolutionName,
                $"A solution project has not been created with the {nameof(PacStepDefinitions.GivenASolutionProjectHasBeenCreatedWithThePowerAppsCLI)} binding.");
        }

        /// <summary>
        /// Gets the path to the project directory created by <see cref="PacStepDefinitions.GivenASolutionProjectHasBeenCreatedWithThePowerAppsCLI"/>.
        /// </summary>
        /// <param name="scenarioCtx">The scenario context.</param>
        /// <returns>The path to the project directory.</returns>
        /// <exception cref="Exception">Thrown if not added to the context.</exception>
        internal static string GetProjectDirectory(this ScenarioContext scenarioCtx)
        {
            return scenarioCtx.Get<string>(
                PacStepDefinitions.ContextVariableProjectDirectory,
                $"A solution project has not been created with the {nameof(PacStepDefinitions.GivenASolutionProjectHasBeenCreatedWithThePowerAppsCLI)} binding.");
        }

        /// <summary>
        /// Gets the version of the tag set by <see cref="GitStepDefinitions.GivenATagHasBeenMadeMatchingTheFormatSolutionX_X_X"/>.
        /// </summary>
        /// <param name="scenarioCtx">The scenario context.</param>
        /// <returns>The tagged version.</returns>
        /// <exception cref="Exception">Thrown if not added to the context.</exception>
        internal static Version GetTaggedSolutionVersion(this ScenarioContext scenarioCtx)
        {
            return scenarioCtx.Get<Version>(
                GitStepDefinitions.ContextVariableTaggedVersion,
                $"A version has not been tagged with the {nameof(GitStepDefinitions.GivenATagHasBeenMadeMatchingTheFormatSolutionX_X_X)} binding.");
        }

        /// <summary>
        /// Gets the known version set by <see cref="MSBuildStepDefinitions.GivenTheCurrentlyCalculatedSolutionVersionIsKnown"/>.
        /// </summary>
        /// <param name="scenarioCtx">The scenario context.</param>
        /// <returns>The known version.</returns>
        /// <exception cref="Exception">Thrown if not added to the context.</exception>
        internal static Version GetKnownVersion(this ScenarioContext scenarioCtx)
        {
            return scenarioCtx.Get<Version>(
                MSBuildStepDefinitions.ContextVariableKnownVersion,
                $"A known version has not been captured with the {nameof(MSBuildStepDefinitions.GivenTheCurrentlyCalculatedSolutionVersionIsKnown)} binding.");
        }

        /// <summary>
        /// Gets the version parts incremented by each commit for the given branch.
        /// </summary>
        /// <param name="scenarioCtx">The scenario context.</param>
        /// <param name="branch">The branch to get the commit version parts for.</param>
        /// <returns>The version parts.</returns>
        /// <exception cref="Exception">Thrown if not added to the context.</exception>
        internal static IEnumerable<VersionPart> GetBranchCommitVersionParts(this ScenarioContext scenarioCtx, string branch)
        {
            return scenarioCtx.Get<IEnumerable<VersionPart>>(
                branch,
                $"Commit version parts for {branch} branch have not been captured with the {nameof(GitStepDefinitions.GivenTheBranchHasReceivedOrMoreCommits)} binding.");
        }

        private static T Get<T>(this ScenarioContext scenarioCtx, string key, string exception)
        {
            if (!scenarioCtx.TryGetValue<T>(key, out var value))
            {
                throw new Exception(exception);
            }

            return value;
        }
    }
}
