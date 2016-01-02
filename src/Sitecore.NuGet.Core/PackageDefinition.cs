using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sitecore.NuGet.Core
{
    public class PackageDefinition
    {
        #region Patterns

        public static readonly IEnumerable<PackageDefinition> PackageDefinitions = new[]
        {
          new PackageDefinition
          { 
            Tag = "Sitecore.Core",
            Id = "Sitecore.Core",
            Title = "Sitecore Kernel Assembly",
            Description = "Main Sitecore Assembly that are required for Sitecore development. ",
            Dependencies = new[]
            {
              "Newtonsoft.Json"
            },
            Assemblies = new[]
            {
              "bin/Sitecore.Kernel.dll"
            }
          },
          new PackageDefinition
          { 
            Tag = "Sitecore.Client",
            Id = "Sitecore.Client",
            Title = "Sitecore Client Assemblies",
            Description = "Main Sitecore Assemblies that are required for Sitecore development within the Sitecore Client. ",
            Dependencies = new[]
            {
              "Sitecore.Core"
            },
            Assemblies = new[]
            {
              "bin/Sitecore.Client.dll"
            }
          },
          new PackageDefinition
          { 
            Tag = "Sitecore.Mvc",
            Id = "Sitecore.Mvc",
            Title = "Sitecore Mvc Assembly",
            Description = "Main Sitecore Assembly that are necessary for Sitecore development with usage of Sitecore MVC. ",
            Dependencies = new []
            {
              "Sitecore.Core",
              "Microsoft.AspNet.Mvc"
            },
            Assemblies = new[]
            {
              "bin/Sitecore.Mvc.dll"
            }
          },
          new PackageDefinition
          { 
            Tag = "Sitecore.ExperienceEditor",
            Id = "Sitecore.ExperienceEditor",
            Title = "Sitecore Experience Editor Assemblies",
            Description = "Main Sitecore Assemblies that are required for Sitecore development within the Experience Editor. ",
            Dependencies = new[]
            {
              "Sitecore.Client"
            },
            Assemblies = new[]
            {
              "bin/Sitecore.ExperienceEditor.dll",
              "bin/Sitecore.ExperienceEditor.Speak.dll"
            }
          },
          new PackageDefinition
          { 
            Tag = "Sitecore.Analytcs",
            Id = "Sitecore.Analytics",
            Title = "Sitecore Analytics Assemblies",
            Description = "Main Sitecore Assemblies that are necessary for Sitecore development with usage of Sitecore Analytics API. ",
            Dependencies = new []
            {
              "Sitecore.Core"
            },
            Assemblies = new[]
            {
              "bin/Sitecore.Analytics.dll", 
              "bin/Sitecore.Analytics.Automation.dll", 
              "bin/Sitecore.Analytics.Core.dll", 
              "bin/Sitecore.Analytics.Model.dll", 
              "bin/Sitecore.Mvc.Analytics.dll"
            }
          },
          //new PackageDefinition
          //{ 
          //  Tag = "Sitecore.Mvc.Analytics",
          //  Id = "Sitecore.Mvc.Analytics",
          //  Title = "Sitecore Mvc Analytics Assemblies",
          //  Description = "Main Sitecore Assemblies that are necessary for Sitecore development with usage of Sitecore MVC and Analytics. ",
          //  Dependencies = new []
          //  {
          //    "Sitecore.Analytics",
          //    "Sitecore.Mvc"
          //  },
          //  Assemblies = new[]
          //  {
          //    "bin/Sitecore.Mvc.Analytics.dll"
          //  }
          //},
          new PackageDefinition
          { 
            Tag = "Sitecore.Buckets",
            Id = "Sitecore.Buckets",
            Title = "Sitecore Buckets Assemblies",
            Description = "Main Sitecore Assemblies that are necessary for Sitecore development with usage of Sitecore Buckets API. ",
            Dependencies = new []
            {
              "Sitecore.Core"
            },
            Assemblies = new[]
            {
              "bin/Sitecore.Buckets.dll"
            }
          },
          new PackageDefinition
          { 
            Tag = "Sitecore.ContentSearch",
            Id = "Sitecore.ContentSearch",
            Title = "Sitecore ContentSearch Assemblies",
            Description = "Main Sitecore Assemblies that are necessary for Sitecore development with usage of Sitecore ContentSearch API. ",
            Dependencies = new []
            {
              "Sitecore.Core"
            },
            Assemblies = new[]
            {
              "bin/Sitecore.ContentSearch.dll",
              "bin/Sitecore.ContentSearch.Linq.dll"
            }
          },
          new PackageDefinition
          { 
            Tag = "Sitecore.ContentSearch.Analytics",
            Id = "Sitecore.ContentSearch.Analytics",
            Title = "Sitecore ContentSearch Assemblies",
            Description = "Main Sitecore Assemblies that are necessary for Sitecore development with usage of Sitecore ContentSearch API. ",
            Dependencies = new []
            {
              "Sitecore.Analytics",
              "Sitecore.ContentSearch"
            },
            Assemblies = new[]
            {
              "bin/Sitecore.ContentSearch.Analytics.dll"
            }
          }
        };
        #endregion

        #region Properties

        public string Tag { get; set; }

        public string Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string[] Assemblies { get; set; }

        public string[] Dependencies { get; set; }

        #endregion
    }
}
