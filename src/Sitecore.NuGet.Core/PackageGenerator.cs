namespace Sitecore.NuGet.Core
{
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.IO;
  using System.Linq;
  using Ionic.Zip;
  using Sitecore.Diagnostics;
  using Sitecore.Diagnostics.Annotations;
  using ManifestDependency = global::NuGet.ManifestDependency;
  using ManifestDependencySet = global::NuGet.ManifestDependencySet;
  using ManifestMetadata = global::NuGet.ManifestMetadata;
  using PackageBuilder = global::NuGet.PackageBuilder;
  using PhysicalPackageFile = global::NuGet.PhysicalPackageFile;

  public class PackageGenerator
  {
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

    [NotNull]
    public virtual IEnumerable<string> Generate([NotNull] string packageFilePath, [NotNull] string outputFolderPath)
    {
      Assert.ArgumentNotNull(packageFilePath, "packageFilePath");
      Assert.ArgumentNotNull(outputFolderPath, "outputFolderPath");

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

        var enumerable = this.Generate(tmp, outputFolderPath, Path.GetFileNameWithoutExtension(packageFilePath));

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
    public virtual IEnumerable<string> Generate([NotNull] string binFolderPath, [NotNull] string outputFolderPath, [NotNull] string releaseTitle)
    {
      Assert.ArgumentNotNull(binFolderPath, "binFolderPath");
      Assert.ArgumentNotNull(outputFolderPath, "outputFolderPath");
      Assert.ArgumentNotNull(releaseTitle, "releaseTitle");

      var binFolder = new DirectoryInfo(binFolderPath);
      Assert.IsTrue(binFolder.Exists, "The folder does not exist: {0}", binFolder.FullName);

      var files = binFolder.GetFiles("Sitecore.*.dll", SearchOption.AllDirectories);
      var ver = FileVersionInfo.GetVersionInfo(Path.Combine(binFolderPath, "Sitecore.Kernel.dll"));
      var releaseVersion = this.GetReleaseVersion(ver);

      foreach (var file in files)
      {
        yield return this.Generate(file.FullName, outputFolderPath, releaseVersion, releaseTitle);
      }

      var nugetFilePath = Path.Combine(outputFolderPath, "Sitecore." + releaseVersion + ".nupkg");
      var dependencies = files.Select(x => new ManifestDependency { Id = this.GetNuGetName(x.FullName), Version = releaseVersion });
      var metadata = new ManifestMetadata
      {
        Id = "Sitecore",
        Version = releaseVersion,
        Authors = "Sitecore",
        Description = "All assemblies of Sitecore " + releaseTitle,
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

      yield return nugetFilePath;
    }

    [NotNull]
    public virtual string Generate([NotNull] string assemblyFilePath, [NotNull] string outputFolderPath, [NotNull] string releaseVersion, [NotNull] string releaseTitle)
    {
      Assert.ArgumentNotNull(assemblyFilePath, "assemblyFilePath");
      Assert.ArgumentNotNull(outputFolderPath, "outputFolderPath");
      Assert.ArgumentNotNull(releaseVersion, "releaseVersion");
      Assert.ArgumentNotNull(releaseTitle, "releaseTitle");

      var tmp = Path.GetTempFileName();
      try
      {
        File.WriteAllText(tmp, InstallPs1);

        var version = FileVersionInfo.GetVersionInfo(assemblyFilePath);
        var nugetName = this.GetNuGetName(assemblyFilePath);
        var nugetFileName = nugetName + "." + releaseVersion + ".nupkg";
        var nugetFilePath = Path.Combine(outputFolderPath, nugetFileName);

        var metadata = new ManifestMetadata
        {
          Id = nugetName,
          Version = releaseVersion,
          Authors = "Sitecore",
          RequireLicenseAcceptance = false,
          Description = string.Format("{0} assembly of {1}, assembly file version: {2}, product version: {3}", nugetName, releaseTitle, version.FileVersion, version.ProductVersion),
        };

        var builder = new PackageBuilder();

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
      finally
      {
        if (File.Exists(tmp))
        {
          File.Delete(tmp);
        }
      }
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
    protected virtual string GetNuGetName([NotNull] string assemblyFilePath)
    {
      Assert.ArgumentNotNull(assemblyFilePath, "assemblyFilePath");

      return Path.GetFileNameWithoutExtension(assemblyFilePath);
    }

    [NotNull]
    protected virtual string GetReleaseVersion([NotNull] FileVersionInfo kernel)
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
