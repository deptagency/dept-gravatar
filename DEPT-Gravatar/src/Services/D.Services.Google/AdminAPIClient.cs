using D.Models.Google.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using D.Models.Common.Configuration;
using System.IO;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Admin.Directory.directory_v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Net.Http;

namespace D.Services.Google
{
    public class AdminAPIClient : IGoogleAdminService
    {
        private GoogleAPIConfiguration _configuration { get; set; }
        private ILogger _logger { get; set; }
        public AdminAPIClient(IOptions<GoogleAPIConfiguration> configuration, ILoggerFactory loggerFactory)
        {
            _configuration = configuration.Value;
            
            _logger = loggerFactory.CreateLogger<AdminAPIClient>();
        }

        /// <summary>
        /// Method used to retrieve all the email addresses and aliases for the user identified with the address/alias passed as parameter
        /// </summary>
        /// <param name="email"></param>
        /// <returns>all the email addresses and aliases (including the email passed as parameter)</returns>
        public async Task<IEnumerable<string>> GetAccountAliases(string email)
        {
            DirectoryService service = getDirectoryService(new string[] { DirectoryService.Scope.AdminDirectoryUserAliasReadonly });
            UsersResource.AliasesResource.ListRequest request = service.Users.Aliases.List(email);
            
            return await request.ExecuteAsync().ContinueWith<IEnumerable<string>>(postTask =>
            {
                if (!postTask.IsFaulted)
                {
                    IList<Alias> aliasesList = postTask?.Result?.AliasesValue;
                    if(aliasesList != null && aliasesList.Any())
                    {
                        HashSet<string> emailsList = new HashSet<string>(aliasesList.Select(a => a.AliasValue));
                        emailsList.UnionWith(aliasesList.Select(a => a.PrimaryEmail));
                        return emailsList.ToList();
                    }
                    else
                    {
                        return new List<string>();
                    }
                }
                else
                {
                    _logger.LogError($"{postTask.Exception.Message} {postTask.Exception.Source} {postTask.Exception.StackTrace}");
                    
                }
                return null;
            });
            
        }

        /// <summary>
        /// Get the initialized directory service (including the credentials)
        /// </summary>
        /// <param name="scopes">Scopes needed by the directory services API call</param>
        /// <returns>DirectoryService</returns>
        private DirectoryService getDirectoryService(IEnumerable<string> scopes)
        {

            var credential = new ServiceAccountCredential(
                new ServiceAccountCredential.Initializer(_configuration.ServiceAccountClientEmail)
                {
                    Scopes = scopes,
                    User = _configuration.UserEmailImpersonate
                }.FromPrivateKey(_configuration.ServiceAccountPrivateKey));

            BaseClientService.Initializer _initializer = new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = _configuration.ApplicationName
            };

            return new DirectoryService(_initializer);
        }
    }
}
