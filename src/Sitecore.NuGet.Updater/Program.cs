namespace Sitecore.NuGet.Updater
{
  using System;
  using System.Diagnostics;
  using System.IO;
  using System.Linq;
  using System.Text.RegularExpressions;
  using Sitecore.NuGet.Core;

  public static class Program
  {
    public static readonly Regex ProductFileNameRegex = new Regex(@"Sitecore\s*(\d)\.(\d)\.?(\d?)\s*rev\.\s*(\d\d\d\d\d\d)", RegexOptions.Compiled);

    private static int Main(string[] args)
    {
      if (args.Length != 2 && args.Length != 3)
      {
        Console.WriteLine("The application expects two or three arguments:");
        Console.WriteLine("1. The path to the specific zip distributive or to the folder with them");
        Console.WriteLine("2. The output folder");
        Console.WriteLine("3. (Optional) NuGet Server credentials and address to push package to (format: user:pass@servername)");

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
      if (!Directory.Exists(p2))
      {
        Directory.CreateDirectory(p2);
      }

      var pushData = ServerInfo.Parse(p3);
      if (Directory.Exists(p1))
      {
        ProcessFolder(p1, p2, pushData);
      }
      else
      {
        ProcessFile(p1, p2, pushData);
      }
    }

    private static void ProcessFolder(string directory, string outputFolderPath, ServerInfo pushData)
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

    private static void ProcessFile(string file, string outputFolderPath, ServerInfo pushData)
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
        if (v == null || v.Major < 0)
        {
          return;
        }
        
        // Create nupkg file
        var nupkgFiles = new PackageGenerator().Generate(file, outputFolderPath);

        // Publish nupkg files
        foreach (var nupkgFile in nupkgFiles)
        {
          PublishPackage(nupkgFile, pushData);
        }
      }
    }

    private static void PublishPackage(string nupkgPath, ServerInfo pushData)
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

    private static string GetSource(ServerInfo pushData)
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
  }
}