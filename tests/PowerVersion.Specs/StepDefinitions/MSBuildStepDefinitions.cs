namespace PowerVersion.Specs.StepDefinitions
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using Defra.PetTravel.Specs.Hooks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using PowerVersion.Specs.Extensions;
    using TechTalk.SpecFlow;

    /// <summary>
    /// Step bindings relating to MSBuild.
    /// </summary>
    [Binding]
    public class MSBuildStepDefinitions
    {
        /// <summary>
        /// The context variable key for the solution version captured by <see cref="GivenTheCurrentlyCalculatedSolutionVersionIsKnown"/>.
        /// </summary>
        public const string ContextVariableKnownVersion = nameof(ContextVariableKnownVersion);

        private const string XPathSolutionManifestVersion = "/ImportExportXml/SolutionManifest/Version";

        private readonly ScenarioContext scenarioCtx;
        private readonly TestContext testCtx;

        /// <summary>
        /// Initializes a new instance of the <see cref="MSBuildStepDefinitions"/> class.
        /// </summary>
        /// <param name="scenarioCtx">The scenario context.</param>
        /// <param name="testCtx">The test context.</param>
        public MSBuildStepDefinitions(ScenarioContext scenarioCtx, TestContext testCtx)
        {
            this.scenarioCtx = scenarioCtx ?? throw new ArgumentNullException(nameof(scenarioCtx));
            this.testCtx = testCtx ?? throw new ArgumentNullException(nameof(testCtx));
        }

        /// <summary>
        /// Adds a package reference to the PowerVersion NuGet package in the solution project.
        /// </summary>
        [Given(@"the PowerVersion NuGet package has been installed")]
        public void GivenThePowerVersionNuGetPackageHasBeenInstalled()
        {
            var projectDirectory = this.scenarioCtx.GetProjectDirectory();
            var localFeed = Path.Combine(Path.GetTempPath(), SetUpHooks.NuGetFeedDirectoryName);

            this.testCtx.WriteLine($"Installing PowerVersion.MSBuild package from {localFeed} feed to project in {projectDirectory}.");

            this.CreateNuGetConfig(projectDirectory, localFeed);

            ExecuteDotNetCommand(
                $"add package PowerVersion.MSBuild --source {localFeed} --no-restore --prerelease",
                projectDirectory);
        }

        /// <summary>
        /// Builds the solution project and captures the outputted solution version.
        /// </summary>
        [Given(@"the currently calculated solution version is known")]
        public void GivenTheCurrentlyCalculatedSolutionVersionIsKnown()
        {
            var projectDirectory = this.scenarioCtx.GetProjectDirectory();
            ExecuteDotNetCommand($"build", projectDirectory);

            var outputDirectory = Path.Combine(projectDirectory, "bin", "Debug");
            var archivePath = Path.Combine(outputDirectory, $"{this.scenarioCtx.GetSolutionName()}.zip");
            var extractDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            this.testCtx.WriteLine($"Extracting solution zip at {archivePath} to {extractDirectory}.");
            ZipFile.ExtractToDirectory(archivePath, extractDirectory);

            var solutionXml = Path.Combine(extractDirectory, "Solution.xml");
            this.testCtx.WriteLine($"Reading solution version from {solutionXml}.");

            var version = new Version(XDocument.Load(solutionXml).XPathSelectElement(XPathSolutionManifestVersion).Value);
            this.testCtx.WriteLine($"Current version is {version}. Adding known version to context");

            this.scenarioCtx.Add(ContextVariableKnownVersion, version);
        }

        /// <summary>
        /// Builds the solution project.
        /// </summary>
        [When(@"the solution project is built")]
        public void WhenTheSolutionProjectIsBuilt()
        {
            ExecuteDotNetCommand($"build", this.scenarioCtx.GetProjectDirectory());
        }

        private static string ExecuteDotNetCommand(string command, string path)
        {
            using (var process = new Process())
            {
                process.StartInfo.FileName = "dotnet";
                process.StartInfo.Arguments = command;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.WorkingDirectory = path;

                process.Start();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    throw new Exception(process.StandardOutput.ReadToEnd());
                }

                return process.StandardOutput.ReadToEnd();
            }
        }

        private void CreateNuGetConfig(string projectDirectory, string localFeed)
        {
            var nugetConfigPath = Path.Combine(projectDirectory, "NuGet.config");
            this.testCtx.WriteLine($"Creating NuGet.config at {nugetConfigPath} to retrieve PowerVersion NuGet from local feed.");

            File.WriteAllText(
                nugetConfigPath,
                $"<?xml version=\"1.0\" encoding=\"utf-8\"?><configuration><packageSources><add key=\"nuget.org\" value=\"https://api.nuget.org/v3/index.json\" /><add key=\"local\" value=\"{localFeed}\" /></packageSources><packageSourceMapping><packageSource key=\"nuget.org\"><package pattern=\"*\" /></packageSource><packageSource key=\"local\"><package pattern=\"PowerVersion.*\" /></packageSource></packageSourceMapping></configuration>");
        }
    }
}
