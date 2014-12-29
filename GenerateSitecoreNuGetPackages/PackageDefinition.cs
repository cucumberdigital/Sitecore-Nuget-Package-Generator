namespace GenerateSitecoreNuGetPackages
{
  using System.Collections.Generic;

  public class PackageDefinition
  {
    #region Patterns

    public static readonly IEnumerable<PackageDefinition> PackageDefinitions = new[]
    {
      new PackageDefinition
      { 
        Tag = "Sitecore",
        Id = "Sitecore",
        Title = "Sitecore Assemblies",
        Description = "All Sitecore Assemblies that are necessary for Sitecore development. ",
        Array = new[]
        {
          "bin/*.dll",
          "bin_Net4/*.dll",
        }
      },
      new PackageDefinition
      { 
        Tag = "SitecoreKernel",
        Id = "SitecoreKernel",
        Title = "Sitecore Kernel Assemblies",
        Description = "Main Sitecore Assemblies that are required for Sitecore development. ",
        Array = new[]
        {
          "bin/Lucene.Net.dll",
          "bin/Sitecore.Kernel.dll",
          "bin/Sitecore.Mvc.dll",
          "bin/Sitecore.ItemWebApi.dll",
          "bin/Sitecore.Logging.dll",
          "bin/Sitecore.Update.dll",
          "bin/Sitecore.Zip.dll",
          "bin_Net4/*.dll"
        }
      },
      new PackageDefinition
      { 
        Tag = "SitecoreClient",
        Id = "SitecoreClient",
        Title = "Sitecore Client Assemblies",
        Description = "Main Sitecore Assemblies that are required for Sitecore development. ",
        Dependencies = new[]
        {
          "SitecoreKernel"
        },
        Array = new[]
        {
          "bin/Sitecore.*Client*.dll",
          "bin/Sitecore.*Shell*.dll",
          "bin/Sitecore.Apps.Loader.dll"
        }
      },
      new PackageDefinition
      { 
        Tag = "SitecoreAnalytcs",
        Id = "SitecoreAnalytics",
        Title = "Sitecore Analytics Assemblies",
        Description = "Main Sitecore Assemblies that are necessary for Sitecore development with usage of Sitecore Analytics API. ",
        Dependencies = new []
        {
          "SitecoreKernel"
        },
        Array = new[]
        {
          "bin/Sitecore.*Analytics*.dll", 
          "bin/Sitecore.Automation*.dll", 
          "bin/Sitecore.SegmentBuilder.dll",
          "bin/*Mongo*"
        }
      },
      new PackageDefinition
      { 
        Tag = "SitecoreBuckets",
        Id = "SitecoreBuckets",
        Title = "Sitecore Buckets Assemblies",
        Description = "Main Sitecore Assemblies that are necessary for Sitecore development with usage of Sitecore Buckets API. ",
        Dependencies = new []
        {
          "SitecoreKernel"
        },
        Array = new[]
        {
          "bin/Sitecore.*Buckets*.dll"
        }
      },
      new PackageDefinition
      { 
        Tag = "SitecoreContentSearch",
        Id = "SitecoreContentSearch",
        Title = "Sitecore ContentSearch Assemblies",
        Description = "Main Sitecore Assemblies that are necessary for Sitecore development with usage of Sitecore ContentSearch API. ",
        Dependencies = new []
        {
          "SitecoreKernel"
        },
        Array = new[]
        {
          "bin/Sitecore.*ContentSearch*.dll"
        }
      }
    };
    #endregion

    #region Properties

    public string Tag { get; set; }

    public string Id { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public string[] Array { get; set; }

    public string[] Dependencies { get; set; }

    #endregion
  }
}