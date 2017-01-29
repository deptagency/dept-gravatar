using D.Models.Google.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Admin.Directory.directory_v1;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using D.Models.Common.Configuration;
using Google.Apis.Services;
using Google.Apis.Admin.Directory.directory_v1.Data;

namespace D.Services.Google
{
    public class AdminAPIClient : IGoogleAdminService
    {
        private GoogleAPIConfiguration _configuration { get; set; }
        private BaseClientService.Initializer _initializer { get; set; }
        private ILogger _logger { get; set; }
        public AdminAPIClient(IOptions<GoogleAPIConfiguration> configuration, ILoggerFactory loggerFactory)
        {
            _configuration = configuration.Value;
            _initializer = new BaseClientService.Initializer
            {
                ApplicationName = configuration.Value.ApplicationName,
                ApiKey = configuration.Value.ApiKey
            };
            _logger = loggerFactory.CreateLogger<AdminAPIClient>();
        }

        public async Task<string[]> GetAccountAliases(string email)
        {
            DirectoryService service = getDefaultService();
            UsersResource.ListRequest request = service.Users.List();


            request.Query = $"email={email}";
            return await request.ExecuteAsync().ContinueWith<string[]>(postTask =>
            {
                return postTask?.Result?.UsersValue?.First().Aliases.ToArray();
            });
            
        }

        private DirectoryService getDefaultService()
        {
            return new DirectoryService(_initializer);
        }
    }
}
