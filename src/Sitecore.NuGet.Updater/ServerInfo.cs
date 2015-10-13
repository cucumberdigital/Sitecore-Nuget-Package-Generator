namespace Sitecore.NuGet.Updater
{
  using System;

  public class ServerInfo
  {
    #region Properties

    public string Server { get; set; }

    public string Credentials { get; set; }

    #endregion

    public static ServerInfo Parse(string text)
    {
      if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(text.Trim()))
      {
        return null;
      }

      try
      {
        var at = text.IndexOf('@');
        return new ServerInfo
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
  }
}