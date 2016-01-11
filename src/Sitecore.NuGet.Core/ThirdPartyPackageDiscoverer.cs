using NuGet;
using Sitecore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Sitecore.NuGet.Core
{
    public class ThirdPartyPackageDiscoverer
    {
        /// <summary>
        /// Discovers the version of the Nuget package used in this version of Sitecore by checking the DLL version,
        /// and attempting to find that package on the public Nuget repository.
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="binFolderPath"></param>
        /// <returns></returns>
        public ManifestDependency FindPublicThirdPartyNugetPackage(string packageName, string binFolderPath)
        {
            // convert package name to DLL name
            string dllName = this.ResolveKnownDLLNameForPackage(packageName);

            string fullDLLPath = Path.Combine(binFolderPath, dllName + ".DLL");
            // find name of DLL in binDirectory
            if (!File.Exists(fullDLLPath))
            {
                Console.WriteLine("--- Assembly for Nuget Package Not Found: " + packageName);
                return null;
            }

            // find the version of that DLL
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(fullDLLPath);
            string version = fvi.FileVersion;
            VersionInfo ver = new VersionInfo(fvi.FileMajorPart, fvi.FileMinorPart, fvi.FileBuildPart, fvi.FilePrivatePart);

            // find equivalent on public Nuget Repo
            return this.SearchForPackage(packageName, ver);
        }

        /// <summary>
        /// Converts the name of a Nuget Package to the DLL name that we want to check in the bin directory.
        /// These are just some of the DLLs in Sitecore that we know come from Nuget packages different to it's own name.
        /// </summary>
        /// <param name="nugetPackageName"></param>
        /// <returns></returns>
        private string ResolveKnownDLLNameForPackage(string nugetPackageName)
        {
            string dllToCheck = nugetPackageName;
            if (nugetPackageName.Equals("Antlr"))
            {
                dllToCheck = "Antlr3.Runtime";
            }
            else if (nugetPackageName.Equals("izenda.ComponentArt.Web.UI"))
            {
                dllToCheck = "ComponentArt.Web.UI";
            }
            else if (nugetPackageName.Equals("Lucene.Net.Contrib"))
            {
                dllToCheck = "Lucene.Net.Contrib.Core";
            }
            else if (nugetPackageName.Equals("mongocsharpdriver"))
            {
                dllToCheck = "MongoDB.Driver";
            }
            else if (nugetPackageName.Equals("Microsoft.AspNet.WebApi"))
            {
                dllToCheck = "System.Web.Http";
            }
            else if (nugetPackageName.StartsWith("Microsoft.AspNet."))
            {
                dllToCheck = nugetPackageName.Replace("Microsoft.AspNet.", "System.Web.");
            }
            else if (nugetPackageName.Equals("YUICompressor.NET"))
            {
                dllToCheck = "Yahoo.Yui.Compressor";
            }

            return dllToCheck;
        }

        /// <summary>
        /// Searches the public Nuget repository for the given package name and version.
        /// </summary>
        /// <param name="nugetPackageID"></param>
        /// <param name="dllVersionInfo"></param>
        /// <returns></returns>
        private ManifestDependency SearchForPackage(string nugetPackageID, VersionInfo dllVersionInfo)
        {
            // http://blog.nuget.org/20130520/Play-with-packages.html
            //Connect to the official package repository
            IPackageRepository repo = PackageRepositoryFactory.Default.CreateRepository("https://packages.nuget.org/api/v2");

            //Get the list of all NuGet packages with the ID provided
            List<IPackage> packages = repo.FindPackagesById(nugetPackageID).ToList();
            if (!packages.Any())
            {
                // error package not found
                Console.WriteLine("- Package Not Found: " + nugetPackageID);
                return null;
            }

            IPackage matchingPackage = this.FindMatchingPackageVersion(packages, dllVersionInfo);
            if (matchingPackage == null) // || IsNotCompleteVersion) - DotNetOpenAuth issue
            {
                Console.WriteLine("--- Version Not Found: " + nugetPackageID + ", " + dllVersionInfo.ToString());
                return null;
            }

            return new ManifestDependency() { Id = matchingPackage.Id, Version = matchingPackage.Version.Version.ToString() };
        }

        /// <summary>
        /// Attempts to find the correct available version of a Nuget package.
        /// It will Try the version {Major, Minor, Build, Revision}, Then {Major, Minor, Build}, then {Major, Minor}, then {Major}
        /// </summary>
        /// <param name="packages"></param>
        /// <param name="dllVersion"></param>
        /// <returns></returns>
        private IPackage FindMatchingPackageVersion(IEnumerable<IPackage> packages, VersionInfo dllVersion)
        {
            IPackage matchingPackage = packages.Where(package => package.Version.Version.Equals(dllVersion)).FirstOrDefault();
            if (matchingPackage == null)
            {
                // Major, Minor, Build
                Version buildVersionMatch = new Version(dllVersion.Major,
                                                 dllVersion.Minor,
                                                 dllVersion.Build, 0);

                matchingPackage = packages.Where(package => package.Version.Version.Equals(buildVersionMatch)).FirstOrDefault();

                if (matchingPackage == null)
                {
                    // Major, Minor, BuildDigit1 (i.e 5.2.30706 becomes 5.2.3)
                    int firstDigitBuildNumber = Math.Abs(dllVersion.Build);
                    while (firstDigitBuildNumber >= 10)
                        firstDigitBuildNumber /= 10;

                    Version firstDigitiBuildVersionMatch = new Version(dllVersion.Major,
                                                     dllVersion.Minor,
                                                     firstDigitBuildNumber, 0);

                    matchingPackage = packages.Where(package => package.Version.Version.Equals(firstDigitiBuildVersionMatch)).FirstOrDefault();

                    if (matchingPackage == null)
                    {

                        // Major, Minor
                        Version minorVersionMatch = new Version(dllVersion.Major,
                                                         dllVersion.Minor, 0, 0);

                        matchingPackage = packages.Where(package => package.Version.Version.Equals(minorVersionMatch)).FirstOrDefault();

                        if (matchingPackage == null)
                        {
                            // Major
                            Version majorVersionMatch = new Version(dllVersion.Major, 0, 0, 0);

                            matchingPackage = packages.Where(package => package.Version.Version.Equals(majorVersionMatch)).FirstOrDefault();
                        }
                    }
                }
            }

            return matchingPackage;
        }

    }
}
