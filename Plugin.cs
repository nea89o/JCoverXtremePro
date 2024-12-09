using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using Jellyfin.Plugin.JCoverXtremePro.Api;
using Jellyfin.Plugin.JellyFed.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.JCoverXtremePro;

public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    public Plugin(
        IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer,
        ILibraryManager libraryManager, ILogger<Plugin> logger,
        IHttpClientFactory httpClientFactory,
        IServerConfigurationManager configurationManager
    ) : base(applicationPaths, xmlSerializer)
    {
        MediuxDownloader.instance = new MediuxDownloader(httpClientFactory);
        Instance = this;
        Logger = logger;
        ScriptInjector.PerformInjection(applicationPaths, configurationManager);
    }

    public override string Name => "JCoverXtremePro";
    public static Guid GUID = Guid.Parse("f3e43e23-4b28-4b2f-a29d-37267e2ea2e2");
    public override Guid Id => GUID;

    public static Plugin? Instance { get; private set; }

    public static ILogger<Plugin> Logger { get; private set; }

    /// <inheritdoc />
    public IEnumerable<PluginPageInfo> GetPages()
    {
        return new[]
        {
            new PluginPageInfo
            {
                Name = Name,
                EmbeddedResourcePath = string.Format(CultureInfo.InvariantCulture, "{0}.Configuration.configPage.html",
                    GetType().Namespace)
            }
        };
    }
}