namespace Defra.PetTravel.Specs.Hooks
{
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using PowerVersion.Specs.Extensions;
    using TechTalk.SpecFlow;

    /// <summary>
    /// Hooks related to teardown.
    /// </summary>
    [Binding]
    public class TearDownHooks
    {
        private readonly ScenarioContext scenarioCtx;
        private readonly TestContext testCtx;

        /// <summary>
        /// Initializes a new instance of the <see cref="TearDownHooks"/> class.
        /// </summary>
        /// <param name="scenarioCtx">The scenario context.</param>
        /// <param name="testCtx">The <see cref="TestContext"/>.</param>
        public TearDownHooks(ScenarioContext scenarioCtx, TestContext testCtx)
        {
            this.scenarioCtx = scenarioCtx;
            this.testCtx = testCtx;
        }

        /// <summary>
        /// Deletes the Git repository (if created).
        /// </summary>
        [AfterScenario(Order = 1000)]
        public void DeleteGitRepository()
        {
            var repositoryDirectory = this.scenarioCtx.GetRepositoryDirectory();

            if (!Directory.Exists(repositoryDirectory))
            {
                this.testCtx.WriteLine("Repository was created as part of this scenario but no longer exists. Exiting clean-up.");
                return;
            }

            NormalizeRepositoryAttributes(repositoryDirectory);

            Directory.Delete(repositoryDirectory, true);

            this.testCtx.WriteLine($"Repository at {repositoryDirectory} was deleted successfully.");
        }

        private static void NormalizeRepositoryAttributes(string path)
        {
            var files = Directory.GetFiles(path);
            var directories = Directory.GetDirectories(path);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
            }

            foreach (string directory in directories)
            {
                NormalizeRepositoryAttributes(directory);
            }

            File.SetAttributes(path, FileAttributes.Normal);
        }
    }
}
