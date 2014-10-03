# Sitecore NuGet Packages Generator #

One of the most commonly questions that Sitecore developers raise is about distributing Sitecore assemblies via NuGet packages for more transparent and easy package management. This project is devoted to resolve this problem by generating your own NuGet packages to publish them on your internal NuGet server. Please note that **SDN license does not allow you to publish those NuGet packages to any public server** such as [www.nuget.org](//www.nuget.org) and you can only use them within the internal network of your company.

### How is it expected to work? ###

* You have a local folder with Sitecore distributive packages in a form of *a ZIP archive of the Sitecore CMS site root* 
* The application extracts necessary assemblies and creates nuget packages according to the pattern
* (Optional) Newly created nuget package is being published to internal NuGet server (for example, [ProGet](http://inedo.com/proget/overview))

### What packages will be generated? ###

* Sitecore
* SitecoreKernel
* SitecoreAnalytics
* SitecoreClient
* SitecoreContentSearch
* SitecoreBuckets

Find details here: [PackageDefinition.cs](http://alienlab.co.uk/sitecore-nuget-packages-generator/src/5a2938d45f5ce455bb98c14ec2cd08e228c7e6c0/GenerateSitecoreNuGetPackages/PackageDefinition.cs?at=master).