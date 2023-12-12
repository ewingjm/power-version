namespace PowerVersion.Core.Models
{
    using System;

    /// <summary>
    /// A solution manifest for a solution being versioned.
    /// </summary>
    public class SolutionManifest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionManifest"/> class.
        /// </summary>
        public SolutionManifest()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionManifest"/> class.
        /// </summary>
        /// <param name="uniqueName">The unique name.</param>
        /// <param name="version">The version.</param>
        public SolutionManifest(string uniqueName, Version version)
        {
            if (string.IsNullOrEmpty(uniqueName))
            {
                throw new ArgumentException($"'{nameof(uniqueName)}' cannot be null or empty.", nameof(uniqueName));
            }

            this.UniqueName = uniqueName;
            this.Version = version.ToString() ?? throw new ArgumentNullException(nameof(version));
        }

        /// <summary>
        /// Gets or sets the unique name of the solution.
        /// </summary>
        public string UniqueName { get; set; }

        /// <summary>
        /// Gets or sets the version of the solution.
        /// </summary>
        public string Version { get; set; }
    }
}
