namespace PowerVersion.Core.Enums
{
    /// <summary>
    /// The parts of a version.
    /// </summary>
    public enum VersionPart
    {
        /// <summary>
        /// The revision version part.
        /// </summary>
        Revision = 0,

        /// <summary>
        /// The build (or patch) version part.
        /// </summary>
        Build = 1,

        /// <summary>
        /// A minor version part.
        /// </summary>
        Minor = 2,

        /// <summary>
        /// A major version part.
        /// </summary>
        Major = 3,
    }
}
