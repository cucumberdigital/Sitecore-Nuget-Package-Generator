namespace GenerateSitecoreNuGetPackages
{
  using System;

  public class NuGetServerInfo
  {
    public static NuGetServerInfo Parse(string text)
    {
      if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(text.Trim()))
      {
        return null;
      }

      try
      {
        var at = text.IndexOf('@');
        return new NuGetServerInfo
        {
          Server = text.Substring(at + 1),
          Credentials = text.Substring(0, at)
        };
      }
      catch (Exception ex)
      {
        throw new FormatException("The format is 'user:pass@server'", ex);
      }
    }

    #region Properties

    public string Server { get; set; }

    public string Credentials { get; set; }

    #endregion
  }
}