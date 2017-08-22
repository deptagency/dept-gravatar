using D.Services.GraphAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using D.Models.Common.Configuration;
using D.Models.GraphAPI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO;
using D.Models.GraphAPI.Interfaces;
using D.Models.Common.Interfaces;
using D.Services.ImageDownloader;
using D.Services.Cryptography;
using System.Collections;
using D.Models.Google.Interfaces;
using D.Services.Google;

namespace D.Applications.GravatarImporter
{
    public class Program
    {
        private static string environment;
        private const string defaultEnvironment = "Development";
        public static void Main(string[] args)
        {
            environment = Environment.GetEnvironmentVariable("DEPT-GRAVATAR_Environment");

            if (args != null && args.Count() == 1)
            {
                environment = args[0];
            }

            if(string.IsNullOrWhiteSpace(environment))
            {
                environment = defaultEnvironment;
            }

            IServiceCollection serviceCollection = new ServiceCollection();

            ConfigureServices(serviceCollection);

            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            var app = serviceProvider.GetService<Application>();

            app.Run().Wait();

        }

        private static void ConfigureServices(IServiceCollection services)
        {
            ILoggerFactory loggerFactory = new LoggerFactory()
                .AddConsole()
                .AddDebug();

            services.AddSingleton(loggerFactory); // Add first my already configured instance
            services.AddLogging(); // Allow ILogger<T>

            IConfigurationRoot configuration = GetConfiguration();
            services.AddSingleton<IConfiguration>(configuration);
            
            // Support typed Options
            services.AddOptions();
            services.Configure<GraphAPIConfiguration>(configuration.GetSection("GraphAPI"));
            services.Configure<GoogleAPIConfiguration>(configuration.GetSection("GoogleAPI"));
            services.Configure<GravatarsConfiguration>(configuration.GetSection("Gravatars"));

            services.AddSingleton<IGraphAPIService, GraphAPIClient>();
            services.AddSingleton<IGoogleAdminService, AdminAPIClient>();
            services.AddSingleton<IDownloadService, ImageDownloader>();
            services.AddSingleton<ICryptographyService, Cryptographer>();
            services.AddTransient<Application>();
            
        }

        private static IConfigurationRoot GetConfiguration()
        {
            IConfigurationBuilder configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{environment.ToLowerInvariant()}.json", optional: true)
                //these EnvironmentVariables will override the appsettings.json and should be set like DEPT-GRAVATAR_GraphAPI:AdminAccessToken
                //Order matters, so don't change the current order!
                //After adding a new Environment variable to the system, restart VS otherwise it won't be able to get it
                //lets select the prefixes of the variables we want. Otherwise it will load ALL environment variables
                .AddEnvironmentVariables("DEPT-GRAVATAR_");

            //poor men's solution for the ASP.NET env.IsDevelopment()
            if (string.Compare(environment, defaultEnvironment, true) == 0)
            {
                //User secrets should only be used in development but console apps do not have an environment
                //avoid using Environment variables during development
                //http://asp.net-hacker.rocks/2016/07/11/user-secrets-in-aspnetcore.html
                configuration.AddUserSecrets<Program>();
            }

            return configuration.Build();
        }
    }
}
