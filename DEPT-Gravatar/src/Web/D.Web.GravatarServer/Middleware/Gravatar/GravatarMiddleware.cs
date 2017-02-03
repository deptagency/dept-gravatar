using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.FileProviders;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.NodeServices;
using Microsoft.Extensions.Primitives;
using System.IO;

namespace D.Web.GravatarServer.Middleware.Gravatar
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class GravatarMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly GravatarOptions _options;
        private readonly IFileProvider _fileProvider;
        private readonly string _defaultGravatarPath;
        private readonly INodeServices _nodeService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private ILogger _logger { get; set; }

        public GravatarMiddleware(RequestDelegate next, IHostingEnvironment hostingEnv, IOptions<GravatarOptions> options, ILoggerFactory loggerFactory, INodeServices nodeService)
        {
            _options = options.Value;
            _hostingEnvironment = hostingEnv;
            _fileProvider = _options.FileProvider ?? ResolveFileProvider(hostingEnv);
            _next = next;
            _defaultGravatarPath = _options.DefaultGravatarPath;
            _nodeService = nodeService;
            _logger = loggerFactory.CreateLogger<GravatarMiddleware>();

            //ensure that the provided default gravatar exists
            IFileInfo _fileInfo = _fileProvider.GetFileInfo(_defaultGravatarPath);
            if (!_fileInfo.Exists)
            {
                throw new ArgumentException($"Unable to find file {_defaultGravatarPath}");
            }
        }

        public async Task Invoke(HttpContext httpContext)
        {

            var path = httpContext.Request.Path;
            if (Regex.IsMatch(path.Value, "/avatar/[a-zA-Z0-9]+(/|.jpg)?", RegexOptions.IgnoreCase))
            {
                int size = 0;
                if (!path.Value.EndsWith(".jpg"))
                {
                    httpContext.Request.Path += ".jpg";
                }
                IFileInfo _originalFileInfo = _fileProvider.GetFileInfo(httpContext.Request.Path);
                if(_originalFileInfo.Exists)
                {
                    if (httpContext.Request.QueryString.HasValue)
                    {
                        StringValues sizeParameter = StringValues.Empty;
                        if (httpContext.Request.Query.TryGetValue("s", out sizeParameter) || httpContext.Request.Query.TryGetValue("size", out sizeParameter))
                        {
                            string rawSize = sizeParameter.First().Trim();
                            if (int.TryParse(rawSize, out size) && size > 0 && size < 200)
                            {
                                string resizeFileName = httpContext.Request.Path.Value.Replace(".jpg", $"_{size}.jpg");
                                IFileInfo _resizeFileInfo = _fileProvider.GetFileInfo(resizeFileName);
                                if (!_resizeFileInfo.Exists)
                                {
                                    try
                                    {
                                        var success = await _nodeService.InvokeAsync<bool>("./Node/resizeImage", _originalFileInfo.PhysicalPath, _resizeFileInfo.PhysicalPath, size, size);
                                        if(!success)
                                        {
                                            resizeFileName = httpContext.Request.Path;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError("Error generating resize", ex);
                                        resizeFileName = httpContext.Request.Path;
                                    }

                                }
                                httpContext.Request.Path = resizeFileName;
                            }
                        }

                    }
                }
                else
                {
                    httpContext.Request.Path = _defaultGravatarPath;
                }
                
            }

            await _next(httpContext);
        }

        internal static IFileProvider ResolveFileProvider(IHostingEnvironment hostingEnv)
        {
            if (hostingEnv.WebRootFileProvider == null)
            {
                throw new InvalidOperationException("Missing FileProvider.");
            }
            return hostingEnv.WebRootFileProvider;
        }

    }


}
