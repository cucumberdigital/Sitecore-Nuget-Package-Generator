# Sitecore Nuget Package Generator

<!--- [x] @mentions, #refs, [links](), **formatting**, and <del>tags</del> supported
- [x] list syntax required (any unordered or ordered list supported)
- [x] this is a complete item
- [ ] this is an incomplete item-->


An extension of <a href="https://bitbucket.org/seanholmesby/sitecore-nuget-packages-generator">Sitecore Nuget Package Generator</a> which adds assembly reference based dependencies

Cucumber to date have been referencing Sitecore Libraries from Nuget Packages in a private MyGet feed, that are versioned based on Sitecore release versions, and contain groupings of logically related libraries, that were originally built using an older version of Sean Holmesby's <a href="https://bitbucket.org/seanholmesby/sitecore-nuget-packages-generator">Sitecore Nuget Package Generator</a>:

**Package:**<br/>
 ![](https://raw.githubusercontent.com/cucumberdigital/Sitecore-Nuget-Package-Generator/master/assets/screenshots/MyGet-Package.jpg)

**Versions:**<br/>
 ![](https://raw.githubusercontent.com/cucumberdigital/Sitecore-Nuget-Package-Generator/master/assets/screenshots/MyGet-Package-Versions.jpg)

**Contents examples:**<br/>
 ![](https://raw.githubusercontent.com/cucumberdigital/Sitecore-Nuget-Package-Generator/master/assets/screenshots/MyGet-Package-Contents-Kernel.jpg)
<br/>(Kernel)

 ![](https://raw.githubusercontent.com/cucumberdigital/Sitecore-Nuget-Package-Generator/master/assets/screenshots/MyGet-Package-Contents-Analytics.jpg)
<br/>(Analytics)<br/>

This approach has worked decently well, but:

-	there have been instances when dependencies between Sitecore libraries haven't been correct, and we've had to modify the groupings of libraries between different versions of Sitecore. 
<br/>A prime example of this was Sitecore 8.1 splitting some of it's Mvc logic, which was all in Sitecore.Mvc, into separate libraries based on features such as Sitecore.Mvc.ExperienceEditor, Sitecore.Mvc.Analytics etc. This meant we had to add a library to one of our Nuget packages, and led to a discussion internally, where we questioned:<br/>Should we create a new grouping? How should it be based? Mvc based libraries? ExperienceEditor based libraries? How many different groupings might we need? How are we going to maintain these groupings between versions of Sitecore?
-	it also means that if, as per above, I want to reference Sitecore.Analytics, the Nuget package would include references for some 15 or so libraries.

Ultimately we came to the realisation that groupings aren't really the best methodology. In typical repository type patterns, a package contains minimal libraries, one, maybe two, but has documented dependencies, that can be chained together to build a full dependency graph, and pull down everything you need. 

The ideal situation for a MyGet feed would be to have all the Sitecore libraries in separate Nuget packages, all based on the version of Sitecore we are building against, that define the dependencies between themselves. 

This would mean the libraries effectively 'group' themselves based on the dependency chain. It would also simplify the number of references each project would have. 

So we looked it this and determined that it should be possible, using Reflection, to investigate the ReferencedAssemblies of each library within a clean Sitecore installation, and match them up to create dependencies within the Nuget packages we were generating.

So ideally, in this example below, the Sitecore.Analytics Nuget package should have dependencies on 9 Sitecore libraries, each in turn being their own separate Nuget package, with their own dependencies:
<br/>
![](https://raw.githubusercontent.com/cucumberdigital/Sitecore-Nuget-Package-Generator/master/assets/screenshots/dotPeek-Assembly-References.jpg)



So we spent some time on this, and managed to modify the original Nuget Package Generator to investigate the ReferencedAssemblies, of each assembly in the base Sitecore ZIP installations, and generate Nuget Dependencies based on these. 

Also, where possible programmatically, we searched Nuget's global repository to match third party dependencies, such as Lucene and System.Web.Mvc (Microsoft.AspNet.Mvc), so the generated Nuget Packages for each Sitecore library look like:
 ![](https://raw.githubusercontent.com/cucumberdigital/Sitecore-Nuget-Package-Generator/master/assets/screenshots/Generated-Packages-w-Dependencies.jpg)
