namespace PowerVersion.Core.Enums
{
    /// <summary>
    /// The type of Git branch.
    /// </summary>
    public enum BranchType
    {
        /// <summary>
        /// The trunk e.g. master or main.
        /// </summary>
        Mainline,

        /// <summary>
        /// A release branch e.g. release/1.0
        /// </summary>
        Release,

        /// <summary>
        /// A feature branch e.g. feature/my-new-feature.
        /// </summary>
        Feature,
    }
}
