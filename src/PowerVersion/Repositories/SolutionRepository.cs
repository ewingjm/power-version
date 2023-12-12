namespace PowerVersion.Repositories
{
    using System.IO;
    using System.Xml.Linq;
    using System.Xml.Serialization;
    using System.Xml.XPath;
    using PowerVersion.Core.Interfaces;
    using PowerVersion.Core.Models;

    /// <summary>
    /// A solution repository.
    /// </summary>
    public class SolutionRepository : ISolutionRepository
    {
        private const string XPathSolutionManifest = "/ImportExportXml/SolutionManifest";

        /// <inheritdoc/>
        public SolutionManifest GetManifest(string metadataDirectory)
        {
            using (var reader = XDocument.Load(Path.Combine(metadataDirectory, "Other", "Solution.xml"))
                .XPathSelectElement(XPathSolutionManifest)
                .CreateReader())
            {
                return new XmlSerializer(typeof(SolutionManifest))
                    .Deserialize(reader) as SolutionManifest;
            }
        }
    }
}
