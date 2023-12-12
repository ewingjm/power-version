namespace PowerVersion.Specs.StepDefinitions
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using PowerVersion.Specs.Extensions;
    using TechTalk.SpecFlow;

    /// <summary>
    /// Step bindings relating to the Power Apps CLI.
    /// </summary>
    [Binding]
    public class PacStepDefinitions
    {
        /// <summary>
        /// The context variable key for the project directory created by <see cref="GivenASolutionProjectHasBeenCreatedWithThePowerAppsCLI"/>.
        /// </summary>
        public const string ContextVariableProjectDirectory = nameof(ContextVariableProjectDirectory);

        /// <summary>
        /// The context variable key for the solution name created by <see cref="GivenASolutionProjectHasBeenCreatedWithThePowerAppsCLI"/>.
        /// </summary>
        public const string ContextVariableSolutionName = nameof(ContextVariableSolutionName);

        private readonly ScenarioContext scenarioCtx;
        private readonly TestContext testCtx;

        /// <summary>
        /// Initializes a new instance of the <see cref="PacStepDefinitions"/> class.
        /// </summary>
        /// <param name="scenarioCtx">The scenario context.</param>
        /// <param name="testCtx">The test context.</param>
        public PacStepDefinitions(ScenarioContext scenarioCtx, TestContext testCtx)
        {
            this.scenarioCtx = scenarioCtx ?? throw new ArgumentNullException(nameof(scenarioCtx));
            this.testCtx = testCtx ?? throw new ArgumentNullException(nameof(testCtx));
        }

        /// <summary>
        /// Creates a solution project (pac solution init) with the Power Apps CLI.
        /// </summary>
        [Given(@"a solution project has been created with the Power Apps CLI")]
        public void GivenASolutionProjectHasBeenCreatedWithThePowerAppsCLI()
        {
            var repositoryDirectory = this.scenarioCtx.GetRepositoryDirectory();
            var solutionName = "pwr_Version_Core";
            var projectDirectory = Directory.CreateDirectory(Path.Combine(repositoryDirectory, solutionName));

            this.testCtx.WriteLine($"Created solution project directory at {projectDirectory.FullName}.");

            ExecutePacCommand("solution init --publisher-name powerversion --publisher-prefix pwr", projectDirectory.FullName);

            this.scenarioCtx.Add(ContextVariableProjectDirectory, projectDirectory.FullName);
            this.scenarioCtx.Add(ContextVariableSolutionName, solutionName);
        }

        private static string ExecutePacCommand(string command, string path)
        {
            using (var process = new Process())
            {
                process.StartInfo.FileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "pac", "pac.exe");
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
    }
}
