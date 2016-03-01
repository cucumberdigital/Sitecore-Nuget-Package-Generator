using System.Reflection;
using System.Runtime.Versioning;
using NuGet;

namespace Sitecore.NuGet.Core
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using Ionic.Zip;
    using Sitecore.Diagnostics.Base;
    using Sitecore.Diagnostics.Base.Annotations;
    using ManifestDependency = global::NuGet.ManifestDependency;
    using ManifestDependencySet = global::NuGet.ManifestDependencySet;
    using ManifestMetadata = global::NuGet.ManifestMetadata;
    using PackageBuilder = global::NuGet.PackageBuilder;
    using PhysicalPackageFile = global::NuGet.PhysicalPackageFile;
    using System;

    public class PackageGenerator
    {
        private DirectoryInfo _binFolder;
        private FileVersionInfo _sitecoreKernelVersion;
        private string _sitecoreReleaseVersion;
        private string _sitecoreReleaseTitle;
        private string _outputFolderPath;


        public const string InstallPs1 = @"param($installPath, $toolsPath, $package, $project)
 
write-host ===================================================
write-host ""Setting 'CopyLocal' to false for the following references:""
 
$asms = $package.AssemblyReferences | %{$_.Name}
 
foreach ($reference in $project.Object.References)
{
    if ($asms -contains $reference.Name + "".dll"")
    {
        Write-Host $reference.Name
        $reference.CopyLocal = $false;
    }
}";

        /// <summary>
        /// Processing a ZIP file
        /// </summary>
        /// <param name="packageFilePath"></param>
        /// <param name="outputFolderPath"></param>
        /// <returns></returns>
        [NotNull]
        public virtual IEnumerable<string> Generate([NotNull] string packageFilePath, [NotNull] string outputFolderPath)
        {
            Assert.ArgumentNotNull(packageFilePath, "packageFilePath");
            Assert.ArgumentNotNull(outputFolderPath, "outputFolderPath");

            _outputFolderPath = outputFolderPath;

            var tmp = Path.GetTempFileName() + ".dir";
            try
            {
                Directory.CreateDirectory(tmp);

                using (var zip = new ZipFile(packageFilePath))
                {
                    foreach (var entry in zip.SelectEntries("*/Website/bin/*.dll"))
                    {
                        var fileName = Path.GetFileName(entry.FileName);
                        var outputFilePath = Path.Combine(tmp, fileName);
                        this.Save(entry, outputFilePath);
                    }
                }

                _sitecoreReleaseTitle = Path.GetFileNameWithoutExtension(packageFilePath);

                var enumerable = this.GeneratePackages(tmp);

                // to prevent finally { ... } running before enumeration starts
                return enumerable.ToArray();
            }
            finally
            {
                if (Directory.Exists(tmp))
                {
                    Directory.Delete(tmp, true);
                }
            }
        }


        [NotNull]
        public virtual IEnumerable<string> GeneratePackages([NotNull] string tempDirFolderPath)
        {
            Assert.ArgumentNotNull(tempDirFolderPath, "tempDirFolderPath");

            _binFolder = new DirectoryInfo(tempDirFolderPath);
            Assert.IsTrue(_binFolder.Exists, "The folder does not exist: {0}", _binFolder.FullName);

            var files = _binFolder.GetFiles("*.dll", SearchOption.AllDirectories);
            //var files = binFolder.GetFiles("Sitecore.*.dll", SearchOption.AllDirectories);
            
            _sitecoreKernelVersion = FileVersionInfo.GetVersionInfo(Path.Combine(tempDirFolderPath, "Sitecore.Kernel.dll"));
            _sitecoreReleaseVersion = GetReleaseVersion(_sitecoreKernelVersion);

            var singleDLLPackages = new List<string>();
            foreach (var file in files)
            {
                singleDLLPackages.Add(GenerateSingleDllPackage(file.FullName));
            }

            var releaseFolderPath = Path.Combine(_outputFolderPath, _sitecoreReleaseVersion);
            if (!Directory.Exists(releaseFolderPath))
            {
                Directory.CreateDirectory(releaseFolderPath);
            }

            var nugetFilePath = Path.Combine(releaseFolderPath, "Sitecore." + _sitecoreReleaseVersion + ".nupkg");
            var dependencies = files.Select(x => new ManifestDependency { Id = GetNuGetName(x.FullName), Version = _sitecoreReleaseVersion });
            var metadata = new ManifestMetadata
            {
                Id = "Sitecore",
                Version = _sitecoreReleaseVersion,
                Authors = "Sitecore",
                Description = "All assemblies of Sitecore " + _sitecoreReleaseTitle,
                RequireLicenseAcceptance = false,
                DependencySets = new List<ManifestDependencySet>
            {
              new ManifestDependencySet
              {
                Dependencies = dependencies.ToList()
              }
            }
            };

            var builder = new PackageBuilder();
            builder.Populate(metadata);
            using (FileStream stream = File.Open(nugetFilePath, FileMode.OpenOrCreate))
            {
                builder.Save(stream);
            }

            return singleDLLPackages;
        }

        [NotNull]
        public virtual string GenerateSingleDllPackage([NotNull] string assemblyFilePath)
        {
            Assert.ArgumentNotNull(assemblyFilePath, "assemblyFilePath");
            
            var tmp = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tmp, InstallPs1);

                var version = FileVersionInfo.GetVersionInfo(assemblyFilePath);
                var nugetName = GetNuGetName(assemblyFilePath);
                var nugetFileName = nugetName + "." + _sitecoreReleaseVersion + ".nupkg";
                var nugetFilePath = Path.Combine(_outputFolderPath, nugetFileName);

                var metadata = new ManifestMetadata
                {
                    Id = nugetName,
                    Version = _sitecoreReleaseVersion,
                    Authors = "Sitecore",
                    RequireLicenseAcceptance = false,
                    Description = string.Format("{0} assembly of {1}, assembly file version: {2}, product version: {3}", nugetName, _sitecoreReleaseTitle, version.FileVersion, version.ProductVersion),
                };

                var builder = new PackageBuilder();

                metadata.DependencySets = new List<ManifestDependencySet>()
                {
                    new ManifestDependencySet()
                    {
                        Dependencies = GetReferenedDependencies(assemblyFilePath)
                    }
                };

                builder.Files.Add(new PhysicalPackageFile
                {
                    SourcePath = assemblyFilePath,
                    TargetPath = "lib\\" + Path.GetFileName(assemblyFilePath)
                });

                builder.Files.Add(new PhysicalPackageFile
                {
                    SourcePath = tmp,
                    TargetPath = "tools\\install.ps1"
                });

                builder.Populate(metadata);

                this.Save(builder, nugetFilePath);

                return nugetFilePath;
            }
            catch (Exception er)
            {
                Console.WriteLine(er.Message);
                Console.ReadLine();

                return null;
            }
            finally
            {
                if (File.Exists(tmp))
                {
                    File.Delete(tmp);
                }
            }
        }

        private List<ManifestDependency> GetReferenedDependencies(string assemblyFilePath)
        {
            var dependecies = new List<ManifestDependency>();

            if (!File.Exists(assemblyFilePath))
            {
                Console.WriteLine("Assembly doesn't exist at - " + assemblyFilePath);
                return dependecies;
            }

            var assembly = Assembly.LoadFrom(assemblyFilePath);
            if (assembly == null) return dependecies;

            var referencedAssemblies = assembly.GetReferencedAssemblies();

            dependecies = GenerateDependencies(referencedAssemblies).ToList();

            return dependecies;
        }

        private IEnumerable<ManifestDependency> GenerateDependencies(IEnumerable<AssemblyName> specifiedDependencies)
        {
            var confirmedDependencies = new List<ManifestDependency>();
            var packageDiscoverer = new ThirdPartyPackageDiscoverer();

            foreach (var dependency in specifiedDependencies)
            {

                if (confirmedDependencies.Select(x => x.Id).Contains(dependency.Name))
                    continue;

                // check if the dependency starts with 'Sitecore', does it exist in the singleDLLpackages 
                // i.e is it a Sitecore package and does it exist in this Sitecore version?
                if (dependency.Name.StartsWith("Sitecore"))
                {
                    confirmedDependencies.Add(new ManifestDependency() { Id = dependency.Name, Version = _sitecoreReleaseVersion});
                }
                else
                {
                    ManifestDependency publicNugetPackage;

                    var files = _binFolder.GetFiles(dependency.Name + ".dll", SearchOption.AllDirectories);
                    if (!files.Any())
                    {
                        publicNugetPackage = packageDiscoverer.FindPublicThirdPartyNugetPackage(dependency);
                    }
                    else
                    {
                        publicNugetPackage = packageDiscoverer.FindPublicThirdPartyNugetPackage(Assembly.LoadFrom(files.FirstOrDefault().FullName).GetName());
                    }
                    

                    // check Public Nuget Repo for packages
                     
                    if (publicNugetPackage != null)
                    {
                        confirmedDependencies.Add(publicNugetPackage);
                    }
                }
            }

            return confirmedDependencies;
        }

        protected virtual void Save([NotNull] PackageBuilder builder, [NotNull] string nugetFilePath)
        {
            Assert.ArgumentNotNull(builder, "builder");
            Assert.ArgumentNotNull(nugetFilePath, "nugetFilePath");

            using (var stream = File.Open(nugetFilePath, FileMode.OpenOrCreate))
            {
                builder.Save(stream);
            }
        }

        protected virtual void Save([NotNull] ZipEntry entry, [NotNull] string outputFilePath)
        {
            Assert.ArgumentNotNull(entry, "entry");
            Assert.ArgumentNotNull(outputFilePath, "outputFilePath");

            using (var input = entry.OpenReader())
            {
                var folderPath = Path.GetDirectoryName(outputFilePath);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                using (Stream output = File.OpenWrite(outputFilePath))
                {
                    this.CopyStream(input, output);
                }
            }
        }

        protected virtual void CopyStream([NotNull] Stream input, [NotNull] Stream output)
        {
            Assert.ArgumentNotNull(input, "input");
            Assert.ArgumentNotNull(output, "output");

            var buffer = new byte[8 * 1024];
            int len;
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, len);
            }
        }

        [NotNull]
        protected static string GetNuGetName([NotNull] string assemblyFilePath)
        {
            Assert.ArgumentNotNull(assemblyFilePath, "assemblyFilePath");

            return Path.GetFileNameWithoutExtension(assemblyFilePath);
        }

        [NotNull]
        protected static string GetReleaseVersion([NotNull] FileVersionInfo kernel)
        {
            Assert.ArgumentNotNull(kernel, "kernel");

            var major = kernel.ProductMajorPart;
            var minor = kernel.FileMinorPart;
            var productVersion = kernel.ProductVersion ?? "000000";
            var revision = productVersion.Substring(productVersion.Length - 6);

            return major + "." + minor + ".0." + revision;
        }
    }
}
