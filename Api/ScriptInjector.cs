namespace Jellyfin.Plugin.JCoverXtremePro.Api;

using System;
using System.IO;
using System.Text.RegularExpressions;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Configuration;
using Microsoft.Extensions.Logging;

/// <summary>
/// Utility for injecting a JavaScript script tag into the Jellyfin web frontend.
/// </summary>
public static class ScriptInjector
{
    public static void PerformInjection(
        IApplicationPaths applicationPaths,
        IServerConfigurationManager configurationManager
    )
    {
        var indexHtmlFilePath = Path.Combine(applicationPaths.WebPath, "index.html");
        if (!File.Exists(indexHtmlFilePath))
        {
            Plugin.Logger.LogWarning("Could not find index html file");
            return;
        }

        var html = File.ReadAllText(indexHtmlFilePath);
        var snippet = GetInjectedSnippet(GetHTTPBasePath(configurationManager));
        if (html.Contains(snippet, StringComparison.InvariantCulture))
        {
            Plugin.Logger.LogInformation("Not injecting existing HTML snippet.");
            return;
        }

        html = Regex.Replace(html, $"<script[^>]*guid=\"{Plugin.GUID}\"[^>]*></script>", string.Empty);
        var bodyEnd = html.LastIndexOf("</body>", StringComparison.InvariantCulture);
        if (bodyEnd < 0)
        {
            Plugin.Logger.LogError("Could not find end of body to inject script");
            return;
        }

        html = html.Insert(bodyEnd, snippet);
        try
        {
            File.WriteAllText(indexHtmlFilePath, html);
            Plugin.Logger.LogInformation("Injected index.html");
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError(e, "Failed to write patched index.html");
        }
    }

    public static string GetHTTPBasePath(IServerConfigurationManager configurationManager)
    {
        var networkConfig = configurationManager.GetConfiguration("network");
        var configType = networkConfig.GetType();
        var baseUrlField = configType.GetProperty("BaseUrl");
        var baseUrl = baseUrlField!.GetValue(networkConfig)!.ToString()!.Trim('/');
        return baseUrl;
    }

    public static string GetScriptUrl(string basePath)
    {
        return basePath + "/JCoverXtremeProStatic/ClientScript";
    }

    public static string GetInjectedSnippet(string basePath)
    {
        return
            $"<script guid=\"{Plugin.GUID}\" plugin=\"{Plugin.Instance!.Name}\" src=\"{GetScriptUrl(basePath)}\" defer></script>";
    }
}