using NuGet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Sitecore.NuGet.Core
{
    /// <summary>
    /// Package Creator for Custom Packages, defined in the PackageDefinition class.
    /// These packages are generated with only dependencies on single-DLL packages.
    /// </summary>
    public class CustomPackageGenerator
    {
        public string GenerateSingleCustomPackage(PackageDefinition definition,
                                                  string binFolderPath, 
                                                  string outputFolderPath, 
                                                  string releaseVersion, 
                                                  IEnumerable<string> singleDLLPackages,
                                                  IEnumerable<string> customPackages)
        {
            // if no dependencies exist, don't create package from definition
            IEnumerable<ManifestDependency> dependencies = this.GenerateDependencies(definition.Dependencies, 
                                                                                     binFolderPath,
                                                                                     releaseVersion, 
                                                                                     singleDLLPackages, 
                                                                                     customPackages);
            if (!dependencies.Any())
            {
                return String.Empty;
            }

            // Create nupkg file
            var packageId = definition.Id + "." + releaseVersion;
            var nugetFilePath = Path.Combine(outputFolderPath, packageId + ".nupkg");

            var metadata = new ManifestMetadata
            {
                Id = definition.Id,
                Title = definition.Id,
                Version = releaseVersion,
                Tags = definition.Tag,
                Authors = "Sitecore",
                Owners = "Sitecore",
                Description = definition.Description,
                RequireLicenseAcceptance = false,
                IconUrl = "http://www.sitecore.net/favicon.ico",
                DependencySets = new List<ManifestDependencySet>
                {
                  new ManifestDependencySet
                  {
                        Dependencies =  dependencies.ToList()
                  }
                }
            };

            var builder = new PackageBuilder();
            builder.Populate(metadata);
            using (FileStream stream = File.Open(nugetFilePath, FileMode.OpenOrCreate))
            {
                builder.Save(stream);
            }

            return nugetFilePath;
        }

        private IEnumerable<ManifestDependency> GenerateDependencies(IEnumerable<string> specifiedDependencies, 
                                                                     string binFolderPath,
                                                                     string sitecoreReleaseVersion, 
                                                                     IEnumerable<string> singleDLLPackages,
                                                                     IEnumerable<string> customPackages)
        {
            var confirmedDependencies = new List<ManifestDependency>();
            var packageDiscoverer = new ThirdPartyPackageDiscoverer();
            foreach (string dependencyPackage in specifiedDependencies)
            {
                // check if the dependency starts with 'Sitecore', does it exist in the singleDLLpackages 
                // i.e is it a Sitecore package and does it exist in this Sitecore version?
                if (dependencyPackage.StartsWith("Sitecore"))
                {

                    if (singleDLLPackages.Any(x => x.EndsWith(dependencyPackage + "." + sitecoreReleaseVersion + ".nupkg")) ||
                        customPackages.Any(x => x.EndsWith(dependencyPackage + "." + sitecoreReleaseVersion + ".nupkg")))
                    {
                        confirmedDependencies.Add(new ManifestDependency() { Id = dependencyPackage, Version = sitecoreReleaseVersion });
                    }
                }
                else
                {
                    // check Public Nuget Repo for packages
                    ManifestDependency publicNugetPackage = packageDiscoverer.FindPublicThirdPartyNugetPackage(dependencyPackage, binFolderPath);

                    if (publicNugetPackage != null)
                    {
                        confirmedDependencies.Add(publicNugetPackage);
                    }
                }
            }

            return confirmedDependencies;
        }

    }
}
