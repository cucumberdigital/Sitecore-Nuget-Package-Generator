namespace GenerateSitecoreNuGetPackages
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.IO;
  using System.Linq;
  using System.Text.RegularExpressions;
  using Ionic.Zip;

  public class Program
  {
    public static readonly Regex ProductFileNameRegex = new Regex(@"Sitecore\s*(\d)\.(\d)\.?(\d?)\s*rev\.\s*(\d\d\d\d\d\d)", RegexOptions.Compiled);

    private static int Main(string[] args)
    {
      if (args.Length != 2 && args.Length != 3)
      {
        Console.WriteLine("The application expects two or three arguments:");
        Console.WriteLine("1. The path to the specific zip distributive or to the folder with them");
        Console.WriteLine("2. The output folder");
        Console.WriteLine("3. (Optional) NuGet Server data to push package to (format: user:pass@servername)");

        return -1;
      }

      Process(
        args.Skip(0).First(),
        args.Skip(1).First(),
        args.Skip(2).FirstOrDefault());

      return 0;
    }

    private static void Process(string p1, string p2, string p3)
    {
      var pushData = NuGetServerInfo.Parse(p3);
      if (Directory.Exists(p1))
      {
        ProcessFolder(p1, p2, pushData);
      }
      else
      {
        ProcessFile(p1, p2, pushData);
      }
    }

    private static void ProcessFolder(string directory, string outputFolderPath, NuGetServerInfo pushData)
    {
      var zipfiles = Directory.GetFiles(directory, "Sitecore *.* rev. *.zip", SearchOption.AllDirectories);
      Console.WriteLine("Files: " + zipfiles.Length);
      foreach (var file in zipfiles)
      {
        try
        {
          ProcessFile(file, outputFolderPath, pushData);
        }
        catch (Exception ex)
        {
          Console.WriteLine("Error processing file " + file + ". " + ex.Message + Environment.NewLine + "Stack trace:" + Environment.NewLine + ex.StackTrace);
        }
      }
    }

    private static void ProcessFile(string file, string outputFolderPath, NuGetServerInfo pushData)
    {
      Console.WriteLine("File: " + file);
      var match = ProductFileNameRegex.Match(file);
      if (!match.Success)
      {
        Console.WriteLine("Skipped (regex)");
      }
      else
      {
        var v = VersionInfo.Parse(match);
        if (v != null && v.Major >= 0)
        {
          foreach (var definition in PackageDefinition.PackageDefinitions)
          {
            var id = string.Format(definition.Id, v.Major, v.Minor, v.Build, v.Revision);
            var title = string.Format(definition.Title, v.Major, v.Minor, v.Build, v.Revision);
            var version = v.ToString();
            var tag = definition.Tag;
            var fullName = tag + version;
            var dependencies = definition.Dependencies;

            Console.WriteLine();
            Console.WriteLine("Tag: " + tag);
            Console.WriteLine("Id: " + id);
            Console.WriteLine("Title: " + title);
            Console.WriteLine("Version: " + version);
            Console.WriteLine("Fullname: " + fullName);

            EnsureFolder(outputFolderPath);

            // Example: C:\NuGet Packages\Sitecore6.2.0.101105
            var baseFolderPath = Path.Combine(outputFolderPath, fullName);

            // Example: C:\NuGet Packages\Sitecore6.2.0.101105\Sitecore6.nuspec
            var nuspecPath = Path.Combine(baseFolderPath, fullName + ".nuspec");

            if (File.Exists(nuspecPath))
            {
              Console.WriteLine("Skipped (exists): " + nuspecPath);
              continue;
            }

            var files = PrepareFiles(file, baseFolderPath, definition);
            if (files == null)
            {
              continue;
            }

            // Copy install.ps1 to set Copy Local = False
            CopyInstallScriptFile(baseFolderPath);

            // Create nuspec file
            var description = GetDescription(definition, files);
            CreateSpecificationFile(nuspecPath, id, v, version, title, description, dependencies, files);

            // Create nupkg file
            var nupkgFilePath = Path.Combine(outputFolderPath, fullName + ".nupkg");
            CreatePackage(nuspecPath, nupkgFilePath);

            // Publish nupkg file
            PublishPackage(nupkgFilePath, pushData);
          }
        }
      }
    }

    private static void CopyInstallScriptFile(string baseFolderPath)
    {
      var installTargetFilePath = Path.Combine(baseFolderPath, "tools", "install.ps1");
      var installSourceFilePath = Path.Combine(Environment.CurrentDirectory, "install.ps1");
      EnsureFolder(Path.GetDirectoryName(installTargetFilePath));
      File.Copy(installSourceFilePath, installTargetFilePath, true);
    }

    private static void CreateSpecificationFile(string nuspecPath, string id, VersionInfo versionInfo, string version, string title, string description, IEnumerable<string> dependencies, IEnumerable<string> files)
    {
      EnsureFolder(Path.GetDirectoryName(nuspecPath));
      string dependenciesString = string.Join(string.Empty, (dependencies ?? new string[0]).Select(x => string.Format(@"<dependency id=""{0}"" version=""{1}"" />", string.Format(x, versionInfo.Major), version)));
      var filesString = string.Join(string.Empty, (files ?? new string[0]).Select(x => string.Format(@"<file src=""lib\{0}"" target=""lib\{0}"" />", x)));

      var contents = string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<package xmlns=""http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd"">
    <metadata>
        <id>{0}</id>
        <version>{1}</version>
        <title>{2}</title>
        <authors>Alen Pelin</authors>
        <owners>Sitecore</owners>
        <iconUrl>http://www.sitecore.net/favicon.ico</iconUrl>
        <requireLicenseAcceptance>false</requireLicenseAcceptance>
        <description>{3}</description>
        <dependencies>
          {4}
        </dependencies>
    </metadata>
    <files>
      {5}
      <file src=""tools\install.ps1"" target=""tools\install.ps1"" />
    </files>
</package>", id, version, title, description, dependenciesString, filesString);

      File.WriteAllText(nuspecPath, contents);
    }

    private static IEnumerable<string> PrepareFiles(string file, string baseFolderPath, PackageDefinition definition)
    {
      var files = new List<string>();
      foreach (var pattern in definition.Assemblies)
      {
        // copy the assemblies
        var path = "*/Website/" + pattern;
        using (var zip = new ZipFile(file))
        {
          foreach (var entry in zip.SelectEntries(path))
          {
            string fileName = Path.GetFileName(entry.FileName);
            if (files.Contains(fileName))
            {
              Console.WriteLine("Skipped (dll): " + fileName);
              continue;
            }

            using (var input = entry.OpenReader())
            {
              var outputFilePath = Path.Combine(baseFolderPath, "lib", fileName);
              EnsureFolder(Path.GetDirectoryName(outputFilePath));
              using (Stream output = File.OpenWrite(outputFilePath))
              {
                CopyStream(input, output);
              }
            }

            files.Add(fileName);
          }
        }
      }

      if (files.Count == 0)
      {
        Console.WriteLine("No files that match pattern. Skipping");
        return null;
      }

      return files;
    }

    private static string GetDescription(PackageDefinition definition, IEnumerable<string> files)
    {
      var separator = ", " + Environment.NewLine;
      var description = definition.Description + "Package includes: " + Environment.NewLine + string.Join(separator, files);
      return description;
    }

    private static void CreatePackage(string nuspecPath, string destFileName)
    {
      var dir = Path.GetDirectoryName(nuspecPath);
      var arguments = string.Format("pack \"{0}\"", Path.GetFileName(nuspecPath));
      var processStartInfo = CreateNuGetProcessStartInfo(arguments);
      processStartInfo.WorkingDirectory = dir;

      var process = System.Diagnostics.Process.Start(processStartInfo);

      // wait for end
      process.WaitForExit();

      var path = Directory.GetFiles(dir, "*.nupkg").First();
      if (File.Exists(destFileName))
      {
        File.Delete(destFileName);
      }

      File.Move(path, destFileName);
    }

    private static void PublishPackage(string nupkgPath, NuGetServerInfo pushData)
    {
      if (pushData == null)
      {
        return;
      }

      Console.WriteLine("Pushing to the server");

      var source = GetSource(pushData);
      var credentials = pushData.Credentials;
      var arguments = string.Format("push \"{0}\" -Source {1} -apikey {2}", nupkgPath, source, credentials);
      var processStartInfo = CreateNuGetProcessStartInfo(arguments);
      var process = System.Diagnostics.Process.Start(processStartInfo);

      // wait for end
      process.WaitForExit();
    }

    private static string GetSource(NuGetServerInfo pushData)
    {
      const string Protocol = "://";

      // add a default http protocol if omitted
      var server = pushData.Server;
      if (!server.Contains(Protocol))
      {
        server = "http" + Protocol + server;
      }

      // add a default /nuget/Default feed if omitted
      var position = server.IndexOf(Protocol) + Protocol.Length;
      if (server.TrimEnd('/').IndexOf("/", position) < 0)
      {
        server = server + "/nuget/Default";
      }

      return server;
    }

    private static ProcessStartInfo CreateNuGetProcessStartInfo(string arguments)
    {
      var nugetExeFilePath = Path.Combine(Environment.CurrentDirectory, "NuGet27.exe");
      var processStartInfo = new ProcessStartInfo(nugetExeFilePath, arguments)
      {
        CreateNoWindow = true,
        WindowStyle = ProcessWindowStyle.Hidden
      };
      return processStartInfo;
    }

    private static void EnsureFolder(string libFolderPath)
    {
      if (!Directory.Exists(libFolderPath))
      {
        Directory.CreateDirectory(libFolderPath);
      }
    }

    private static void CopyStream(Stream input, Stream output)
    {
      byte[] buffer = new byte[8 * 1024];
      int len;
      while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
      {
        output.Write(buffer, 0, len);
      }
    }
  }
}