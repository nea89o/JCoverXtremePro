using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.JCoverXtremePro.Api;

using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Static file server for the JavaScript snippet injected by <see cref="ScriptInjector"/>
/// </summary>
[ApiController]
[Route("JCoverXtremeProStatic")]
public class JCoverStaticProvider : ControllerBase
{
    private readonly Assembly assembly;
    private readonly string scriptPath;

    public JCoverStaticProvider()
    {
        assembly = Assembly.GetExecutingAssembly();
        scriptPath = GetType().Namespace + ".coverscript.js";
    }

    [HttpGet("ClientScript")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces("application/javascript")]
    public ActionResult GetClientScript()
    {
        Plugin.Logger.LogInformation($"Requesting ClientScript {scriptPath}");
        var scriptStream = assembly.GetManifestResourceStream(scriptPath);
        if (scriptStream == null)
        {
            return NotFound();
        }

        return File(scriptStream, "application/javascript");
    }
}