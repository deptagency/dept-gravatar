using D.Models.Common.Configuration;
using D.Models.Common.Interfaces;
using D.Models.GraphAPI;
using D.Models.GraphAPI.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using D.Models.Google.Interfaces;

namespace D.Applications.GravatarImporter
{
    public class Application
    {
        private ILogger _logger;
        private IGraphAPIService _fbClient;
        private IGoogleAdminService _googleAdminClient;
        private IConfiguration _configuration;
        private IDownloadService _downloadService;
        private ICryptographyService _cryptographyService;
        private GravatarsConfiguration _gravatarsConfiguration;

        public Application(IConfiguration configuration, IOptions<GravatarsConfiguration> gravatarsConfiguration, ILogger<Application> logger, IGraphAPIService fbClient, IDownloadService downloadService, ICryptographyService cryptographyService, IGoogleAdminService googleAdminClient)
        {
            _logger = logger;
            _fbClient = fbClient;
            _configuration = configuration;
            _downloadService = downloadService;
            _cryptographyService = cryptographyService;
            _gravatarsConfiguration = gravatarsConfiguration.Value;
            _googleAdminClient = googleAdminClient;
        }

        public async Task Run()
        {
            try
            {
                
                string[] fields = new string[] { "id", "first_name", "last_name", "email", "picture.type(large)", "link", "locale", "name", "name_format", "updated_time" };

                List<Member> members = await _fbClient.GetAllCommunityMembers(fields);

                if(string.IsNullOrWhiteSpace(_gravatarsConfiguration.DefaultGravatarRelativePath))
                {
                    throw new MissingFieldException($"{nameof(_gravatarsConfiguration.DefaultGravatarRelativePath)} cannot be null, empty or white space");
                }

                if (string.IsNullOrWhiteSpace(_gravatarsConfiguration.DestinationFolder))
                {
                    throw new MissingFieldException($"{nameof(_gravatarsConfiguration.DestinationFolder)} cannot be null, empty or white space");
                }

                if (!Directory.Exists(_gravatarsConfiguration.DestinationFolder))
                {
                    Directory.CreateDirectory(_gravatarsConfiguration.DestinationFolder);
                }
                string defaultGravatarFullPath = Path.Combine(Directory.GetCurrentDirectory(), _gravatarsConfiguration.DefaultGravatarRelativePath);
                if(!File.Exists(defaultGravatarFullPath))
                {
                    throw new FileNotFoundException($"Could not find the file {defaultGravatarFullPath}");
                }
                byte[] defaultGravatar = File.ReadAllBytes(Path.Combine(Directory.GetCurrentDirectory(), _gravatarsConfiguration.DefaultGravatarRelativePath));
                List<string> userImageFileNames = new List<string>();
                foreach (Member member in members)
                {
                    if(member.Picture != null && member.Picture.Data != null && !string.IsNullOrWhiteSpace(member.Picture.Data.Url))
                    {
                        
                        byte[] image;
                        if (!member.Picture.Data.IsSilhouette)
                        {
                            image = await _downloadService.Download(member.Picture.Data.Url);
                        }
                        else
                        {
                            image = defaultGravatar;
                        }
                        
                        string facebookUserEmail = member.Email.Trim().ToLowerInvariant();
                        //string[] emailAliases = await _googleAdminClient.GetAccountAliases(facebookUserEmail);
                        string[] emailAliases = new string[] { facebookUserEmail };
                        foreach (string userEmailAlias in emailAliases)
                        {
                            string userEmailMD5 = _cryptographyService.CalculateMD5Hash(userEmailAlias);
                            string userEmailMD5FileName = $"{userEmailMD5}.jpg";
                            string fullFileName = Path.Combine(_gravatarsConfiguration.DestinationFolder, userEmailMD5FileName);
                            File.WriteAllBytes(fullFileName, image);
                            _logger.LogInformation($"User {member.Email} picture has been saved as {fullFileName}");
                            userImageFileNames.Add(userEmailMD5FileName);
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"User {member.Email} picture is null, empty or white space");
                    }
                }
                cleanUpFiles(userImageFileNames);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
        }

        private void cleanUpFiles(List<string> userImageFileNames)
        {
            string[] allFiles = Directory.GetFiles(_gravatarsConfiguration.DestinationFolder, "*.jpg", SearchOption.TopDirectoryOnly);
            IEnumerable<string> filesToRemove = allFiles.Where(f => !userImageFileNames.Any(ui => f.EndsWith($"{Path.DirectorySeparatorChar}{ui}")));
            foreach (string file in filesToRemove)
            {
                File.Delete(file);
            }
        }
    }
}
