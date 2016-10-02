using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace D.Web.GravatarServer.Middleware.Gravatar
{
    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class GravatarMiddlewareExtensions
    {
        public static IApplicationBuilder UseGravatarMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<GravatarMiddleware>();
        }

        /// <summary>
        /// Enables gravatar path transformations logic with the given options
        /// </summary>
        /// <param name="app"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseGravatarMiddleware(this IApplicationBuilder app, GravatarOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return app.UseMiddleware<GravatarMiddleware>(Options.Create(options));
        }
    }
}
