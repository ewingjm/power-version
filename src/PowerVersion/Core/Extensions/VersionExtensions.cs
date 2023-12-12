namespace PowerVersion.Core.Extensions
{
    using System;
    using PowerVersion.Core.Enums;

    /// <summary>
    /// Extensions for the <see cref="Version"/> class.
    /// </summary>
    public static class VersionExtensions
    {
        /// <summary>
        /// Gets the last incremented part for a version.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="defaultPart">The default if the version has never been incremented.</param>
        /// <returns>The part or null if version is zero.</returns>
        public static VersionPart GetLastIncrementedPartOrDefault(this Version version, VersionPart defaultPart = VersionPart.Build)
        {
            if (version.Revision > 0)
            {
                return VersionPart.Revision;
            }
            else if (version.Build > 0)
            {
                return VersionPart.Build;
            }
            else if (version.Minor > 0)
            {
                return VersionPart.Minor;
            }
            else if (version.Major > 0)
            {
                return VersionPart.Major;
            }

            return defaultPart;
        }

        /// <summary>
        /// Returns a new version with an increment added from the specified part.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="versionPart">The part to decrement.</param>
        /// <param name="increment">The amount to increment the part by.</param>
        /// <returns>The new version.</returns>
        public static Version IncrementPart(this Version version, VersionPart versionPart, int increment = 1)
        {
            switch (versionPart)
            {
                case VersionPart.Major:
                    return new Version(Math.Max(0, version.Major) + increment, 0, 0);
                case VersionPart.Minor:
                    return new Version(Math.Max(0, version.Major), Math.Max(0, version.Minor) + increment, 0);
                case VersionPart.Build:
                    return new Version(Math.Max(0, version.Major), Math.Max(0, version.Minor), Math.Max(0, version.Build) + increment);
                default:
                    return new Version(Math.Max(0, version.Major), Math.Max(0, version.Minor), Math.Max(0, version.Build), Math.Max(0, version.Revision) + increment);
            }
        }

        /// <summary>
        /// Returns a new version with an decrement subtracted from the specified part.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="versionPart">The part to decrement.</param>
        /// <param name="decrement">The amount to decrement the part by.</param>
        /// <returns>The new version.</returns>
        public static Version DecrementPart(this Version version, VersionPart versionPart, int decrement = 1)
        {
            switch (versionPart)
            {
                case VersionPart.Major:
                    return new Version(Math.Max(0, version.Major - decrement), 0, 0);
                case VersionPart.Minor:
                    return new Version(Math.Max(0, version.Major), Math.Max(0, version.Minor - decrement), 0);
                case VersionPart.Build:
                    return new Version(Math.Max(0, version.Major), Math.Max(0, version.Minor), Math.Max(0, version.Build - decrement));
                default:
                    return new Version(Math.Max(0, version.Major), Math.Max(0, version.Minor), Math.Max(0, version.Build), Math.Max(0, version.Revision - decrement));
            }
        }
    }
}
