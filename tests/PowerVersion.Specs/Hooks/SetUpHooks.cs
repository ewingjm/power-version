namespace Defra.PetTravel.Specs.Hooks
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using TechTalk.SpecFlow;

    /// <summary>
    /// Hooks related to setup.
    /// </summary>
    [Binding]
    public class SetUpHooks
    {
        /// <summary>
        /// The name of the NuGet directory feed created in the %TEMP% directory.
        /// </summary>
        public const string NuGetFeedDirectoryName = "PowerVersion.Specs-PackageFeed";

        private const string ContextVariableLocalFeedDirectory = nameof(ContextVariableLocalFeedDirectory);
        private const string PatternNuGetPackages = "PowerVersion.MSBuild.*.nupkg";

        /// <summary>
        /// Deletes the local NuGet package feed.
        /// </summary>
        [BeforeTestRun(Order = -1000)]
        public static void DeleteLocalNuGetPackageFeed()
        {
            var localFeedDirectory = Path.Combine(Path.GetTempPath(), NuGetFeedDirectoryName);

            if (!Directory.Exists(localFeedDirectory))
            {
                return;
            }

            Directory.Delete(localFeedDirectory, true);
        }

        /// <summary>
        /// Publishes the PowerVersion.MSBuild NuGet package to a local feed.
        /// </summary>
        [BeforeTestRun(Order = -999)]
        public static void PublishNuGetPackageToLocalFeed()
        {
            var localFeedDirectory = Path.Combine(Path.GetTempPath(), NuGetFeedDirectoryName);

            Directory.CreateDirectory(localFeedDirectory);

            var nugetPackage = GetNuGetPackageFile();

            ExecuteDotNetCommand(
                $"nuget push {nugetPackage} --source {localFeedDirectory}",
                Path.GetDirectoryName(nugetPackage));
        }

        /// <summary>
        /// Deletes any versions of Power Version in the global packages cache.
        /// </summary>
        [BeforeTestRun(Order = -998)]
        public static void DeleteCachedPowerVersionPackages()
        {
            string userProfile = Environment.GetEnvironmentVariable("USERPROFILE");
            string nuGetCachePath = Path.Combine(userProfile, ".nuget", "packages", "powerversion.msbuild");

            if (!Directory.Exists(nuGetCachePath))
            {
                return;
            }

            Directory.Delete(nuGetCachePath, true);
        }

        private static string GetNuGetPackageFile()
        {
            var packages = Directory
                .GetFiles(AppDomain.CurrentDomain.BaseDirectory, PatternNuGetPackages)
                .Where(fileName => !fileName.Contains("symbols"))
                .ToArray();

            switch (packages.Length)
            {
                case 0:
                    throw new Exception($"No PowerVersion.MSBuild.*.nupkg files were found in the {AppDomain.CurrentDomain.BaseDirectory} directory.");
                case 1:
                    return packages[0];
                default:
                    return packages.OrderByDescending(p => File.GetCreationTimeUtc(p)).FirstOrDefault();
            }
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
                    throw new Exception(process.StandardError.ReadToEnd());
                }

                return process.StandardOutput.ReadToEnd();
            }
        }
    }
}
