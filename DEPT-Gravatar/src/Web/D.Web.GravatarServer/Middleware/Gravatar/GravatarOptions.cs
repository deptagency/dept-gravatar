using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace D.Web.GravatarServer.Middleware.Gravatar
{
    public class GravatarOptions
    {
        //Full file path to the 
        public string DefaultGravatarPath { get; set; }
        public IFileProvider FileProvider { get; set; }

        public GravatarOptions()
        {
            //Set the DefaultGravatarPath's default in case there isn't any provided
            DefaultGravatarPath = "/avatar/default-avatar.jpg";
        }
    }
}
