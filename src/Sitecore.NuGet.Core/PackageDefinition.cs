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
            Tag = "Sitecore.CoreGroup",
            Id = "Sitecore.CoreGroup",
            Title = "Sitecore Core Assemblies",
            Description = "Main Sitecore Assemblies that are required for Sitecore development. ",
            Dependencies = new[]
            {
              "Newtonsoft.Json",
              "Sitecore.Kernel"
            },
            Assemblies = new string[]
            {
            }
          },
          new PackageDefinition
          { 
            Tag = "Sitecore.Client.CoreGroup",
            Id = "Sitecore.Client.CoreGroup",
            Title = "Sitecore Client Assemblies",
            Description = "Main Sitecore Assemblies that are required for Sitecore development within the Sitecore Client. ",
            Dependencies = new[]
            {
              "Sitecore.CoreGroup",
              "Sitecore.Client"
            },
            Assemblies = new string[]
            {
            }
          },
          new PackageDefinition
          { 
            Tag = "Sitecore.Mvc.CoreGroup",
            Id = "Sitecore.Mvc.CoreGroup",
            Title = "Sitecore Mvc Core Assemblies",
            Description = "Main Sitecore Assemblies that are necessary for Sitecore development with usage of Sitecore MVC. ",
            Dependencies = new []
            {
              "Sitecore.CoreGroup",
              "Sitecore.Mvc",
              "Microsoft.AspNet.Mvc"
            },
            Assemblies = new string[]
            {
            }
          },
          new PackageDefinition
          { 
            Tag = "Sitecore.ExperienceEditor.CoreGroup",
            Id = "Sitecore.ExperienceEditor.CoreGroup",
            Title = "Sitecore Experience Editor Core Assemblies",
            Description = "Main Sitecore Assemblies that are required for Sitecore development within the Experience Editor. ",
            Dependencies = new[]
            {
              "Sitecore.Client.CoreGroup",
              "Sitecore.ExperienceEditor",
              "Sitecore.ExperienceEditor.Speak"
            },
            Assemblies = new string[]
            {
            }
          },
          new PackageDefinition
          { 
            Tag = "Sitecore.Analytcs.CoreGroup",
            Id = "Sitecore.Analytics.CoreGroup",
            Title = "Sitecore Analytics Assemblies",
            Description = "Main Sitecore Assemblies that are necessary for Sitecore development with usage of Sitecore Analytics API. ",
            Dependencies = new []
            {
              "Sitecore.CoreGroup",
              "Sitecore.Analytics", 
              "Sitecore.Analytics.Automation", 
              "Sitecore.Analytics.Core", 
              "Sitecore.Analytics.Model"
            },
            Assemblies = new string[]
            {
            }
          },
          new PackageDefinition
          { 
            Tag = "Sitecore.Mvc.Analytics.CoreGroup",
            Id = "Sitecore.Mvc.Analytics.CoreGroup",
            Title = "Sitecore Mvc Analytics Assemblies",
            Description = "Main Sitecore Assemblies that are necessary for Sitecore development with usage of Sitecore MVC and Analytics. ",
            Dependencies = new []
            {
              "Sitecore.Analytics.CoreGroup",
              "Sitecore.Mvc.CoreGroup",
              "Sitecore.Mvc.Analytics"
            },
            Assemblies = new string[]
            {
            }
          },
          new PackageDefinition
          { 
            Tag = "Sitecore.Buckets.CoreGroup",
            Id = "Sitecore.Buckets.CoreGroup",
            Title = "Sitecore Buckets Assemblies",
            Description = "Main Sitecore Assemblies that are necessary for Sitecore development with usage of Sitecore Buckets API. ",
            Dependencies = new []
            {
              "Sitecore.CoreGroup",
              "Sitecore.Buckets"
            },
            Assemblies = new string[]
            {
            }
          },
          new PackageDefinition
          { 
            Tag = "Sitecore.ContentSearch.CoreGroup",
            Id = "Sitecore.ContentSearch.CoreGroup",
            Title = "Sitecore ContentSearch Assemblies",
            Description = "Main Sitecore Assemblies that are necessary for Sitecore development with usage of Sitecore ContentSearch API. ",
            Dependencies = new []
            {
              "Sitecore.CoreGroup",
              "Sitecore.ContentSearch",
              "Sitecore.ContentSearch.Linq"
            },
            Assemblies = new string[]
            {
            }
          },
          new PackageDefinition
          { 
            Tag = "Sitecore.Speak.CoreGroup",
            Id = "Sitecore.Speak.CoreGroup",
            Title = "Sitecore Speak Assemblies",
            Description = "Main Sitecore Assemblies that are necessary for Sitecore development with usage of Sitecore Speak. ",
            Dependencies = new []
            {
              "Sitecore.CoreGroup",
              "Sitecore.Mvc.CoreGroup",
              "Sitecore.Speak.Mvc",
              "Sitecore.Speak.Client",
              "Sitecore.Speak.Components"
            },
            Assemblies = new string[]
            {
            }
          },
          new PackageDefinition
          { 
            Tag = "Sitecore.Services.Client.CoreGroup",
            Id = "Sitecore.Services.Client.CoreGroup",
            Title = "Sitecore Services Client Assemblies",
            Description = "Main Sitecore Assemblies that are necessary for Sitecore development with usage of Sitecore Services Client. ",
            Dependencies = new []
            {
              "Sitecore.CoreGroup",
              "Sitecore.Services.Client",
              "Sitecore.Services.Core",
              "Sitecore.Services.Infrastructure",
              "Sitecore.Services.Infrastructure.Sitecore"
            },
            Assemblies = new string[]
            {
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
