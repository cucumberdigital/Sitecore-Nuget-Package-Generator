namespace Sitecore.NuGet.Core
{
  using System;
  using System.Text.RegularExpressions;

  public class VersionInfo
  {
    public VersionInfo()
    {
    }

    public int Major { get; set; }

    public int Minor { get; set; }

    public int Build { get; set; }

    public string Revision { get; set; }

    public static VersionInfo Parse(Match match)
    {
      var major = int.Parse(match.Groups[1].Value);
      var minor = int.Parse(match.Groups[2].Value);

      var build = 0;
      try
      {
        build = int.Parse(match.Groups[3].Value);
      }
      catch
      {
      }
      var revision = match.Groups[4].Value;
      int revisionInt;
      if (!int.TryParse(revision, out revisionInt) && revision.Length != 6)
      {
        Console.WriteLine("Skipped (revision): " + revision);
        return null;
      }

      return new VersionInfo
      {
        Major = major,
        Minor = minor,
        Build = build,
        Revision = revision
      };
    }

    /// <summary>
    /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
    /// </returns>
    public override string ToString()
    {
      return string.Format("{0}.{1}.{2}.{3}", this.Major, this.Minor, this.Build, this.Revision);
    }
  }
}