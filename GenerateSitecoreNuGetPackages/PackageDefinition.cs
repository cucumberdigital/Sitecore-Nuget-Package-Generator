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
        Id = "Sitecore{0}",
        Title = "Sitecore {0}",
        Description = "All Sitecore Assemblies that are necessary for Sitecore development. ",
        Array = new[]
        {
          "*.dll"
        }
      },
      new PackageDefinition
      { 
        Tag = "SitecoreKernel",
        Id = "SitecoreKernel{0}",
        Title = "Sitecore Kernel {0}",
        Description = "Main Sitecore Assemblies that are required for Sitecore development. ",
        Array = new[]
        {
          "Lucene.Net.dll",
          "Sitecore.Kernel.dll",
          "Sitecore.Mvc.dll",
          "Sitecore.ItemWebApi.dll",
          "Sitecore.Logging.dll",
          "Sitecore.Update.dll",
          "Sitecore.Zip.dll"
        }
      },
      new PackageDefinition
      { 
        Tag = "SitecoreClient",
        Id = "SitecoreClient{0}",
        Title = "Sitecore Client {0}",
        Description = "Main Sitecore Assemblies that are required for Sitecore development. ",
        Dependencies = new []
        {
          "SitecoreKernel{0}"
        },
        Array = new[]
        {
          "Sitecore.*Client*.dll",
          "Sitecore.*Shell*.dll",
          "Sitecore.Apps.Loader.dll"
        }
      },
      new PackageDefinition
      { 
        Tag = "SitecoreAnalytcs",
        Id = "SitecoreAnalytics{0}",
        Title = "Sitecore Analytics {0}",
        Description = "Main Sitecore Assemblies that are necessary for Sitecore development with usage of Sitecore Analytics API. ",
        Dependencies = new []
        {
          "SitecoreKernel{0}"
        },
        Array = new[]
        {
          "Sitecore.*Analytics*.dll", 
          "Sitecore.Automation*.dll", 
          "Sitecore.SegmentBuilder.dll",
          "*Mongo*"
        }
      },
      new PackageDefinition
      { 
        Tag = "SitecoreBuckets",
        Id = "SitecoreBuckets{0}",
        Title = "Sitecore Buckets {0}",
        Description = "Main Sitecore Assemblies that are necessary for Sitecore development with usage of Sitecore Buckets API. ",
        Dependencies = new []
        {
          "SitecoreKernel{0}"
        },
        Array = new[]
        {
          "Sitecore.*Buckets*.dll"
        }
      },
      new PackageDefinition
      { 
        Tag = "SitecoreContentSearch",
        Id = "SitecoreContentSearch{0}",
        Title = "Sitecore ContentSearch {0}",
        Description = "Main Sitecore Assemblies that are necessary for Sitecore development with usage of Sitecore ContentSearch API. ",
        Dependencies = new []
        {
          "SitecoreKernel{0}"
        },
        Array = new[]
        {
          "Sitecore.*ContentSearch*.dll"
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