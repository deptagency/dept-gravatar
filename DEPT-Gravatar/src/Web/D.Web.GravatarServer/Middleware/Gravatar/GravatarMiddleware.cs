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

namespace D.Web.GravatarServer.Middleware.Gravatar
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class GravatarMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly GravatarOptions _options;
        private readonly IFileProvider _fileProvider;
        private readonly string _defaultGravatarPath;
        
        public GravatarMiddleware(RequestDelegate next, IHostingEnvironment hostingEnv, IOptions<GravatarOptions> options, ILoggerFactory loggerFactory)
        {
            _options = options.Value;
            _fileProvider = _options.FileProvider ?? ResolveFileProvider(hostingEnv);
            _next = next;
            _defaultGravatarPath = _options.DefaultGravatarPath;
            //ensure that the provided default gravatar exists
            IFileInfo _fileInfo = _fileProvider.GetFileInfo(_defaultGravatarPath);
            if (!_fileInfo.Exists)
            {
                throw new ArgumentException($"Unable to find file {_defaultGravatarPath}");
            }
        }

        public Task Invoke(HttpContext httpContext)
        {
            
            var path = httpContext.Request.Path;
            if (Regex.IsMatch(path.Value,"/avatar/.+",RegexOptions.IgnoreCase))
            {
                if(!path.Value.EndsWith(".jpg"))
                {
                    httpContext.Request.Path += ".jpg";
                }
                
                IFileInfo _fileInfo = _fileProvider.GetFileInfo(httpContext.Request.Path);
                if(!_fileInfo.Exists)
                {
                    httpContext.Request.Path = _defaultGravatarPath;
                }
                
            }
            return _next(httpContext);
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
