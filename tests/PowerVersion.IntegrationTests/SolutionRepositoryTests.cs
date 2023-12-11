namespace PowerVersion.IntegrationTests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using PowerVersion.Repositories;

    /// <summary>
    /// Integration tests for the <see cref="solutionRepository"/> class.
    /// </summary>
    [TestClass]
    public class SolutionRepositoryTests
    {
        private readonly SolutionRepository solutionRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionRepositoryTests"/> class.
        /// </summary>
        public SolutionRepositoryTests()
        {
            this.solutionRepository = new SolutionRepository();
        }

        /// <summary>
        /// Asserts that <see cref="SolutionRepository.GetManifest(string)"/> returns the correct solution name.
        /// </summary>
        [TestMethod]
        public void GetManifest_ValidMetadataDirectory_ReturnsSolutionManifestName()
        {
            var actualManifest = this.solutionRepository.GetManifest("Metadata");

            Assert.AreEqual(new Version(1, 0).ToString(), actualManifest.Version);
        }

        /// <summary>
        /// Asserts that <see cref="SolutionRepository.GetManifest(string)"/> returns the correct solution version.
        /// </summary>
        [TestMethod]
        public void GetManifest_ValidMetadataDirectory_ReturnsSolutionManifestVersion()
        {
            var actualManifest = this.solutionRepository.GetManifest("Metadata");

            Assert.AreEqual("pwr_Version_Core", actualManifest.UniqueName);
        }
    }
}
