using D.Models.Common.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using D.Models.GraphAPI;
using Microsoft.Extensions.Options;
using D.Models.GraphAPI.Interfaces;
using Microsoft.Extensions.Logging;

namespace D.Services.GraphAPI
{
    public class GraphAPIClient : IGraphAPIService
    {
        private GraphAPIConfiguration _configuration { get; set; }
        private ILogger _logger { get; set; }
        public GraphAPIClient(IOptions<GraphAPIConfiguration> configuration, ILoggerFactory loggerFactory)
        {
            _configuration = configuration.Value;
            _logger = loggerFactory.CreateLogger<GraphAPIClient>();
        }

        public async Task<Response<Member>> GetCommunityMembers(string[] fields = null, int limit = 25)
        {
            HttpClient client = getClient();
            Dictionary<string, string[]> requestParameters = new Dictionary<string, string[]>();
            requestParameters.Add(nameof(limit), new string[] { limit.ToString() });
            if(fields != null)
            {
                requestParameters.Add(nameof(fields), fields);
            }
            string parameters = generateQueryString(requestParameters);
            _logger.LogInformation($"Request: {this._configuration.CommunityMembersPath}{parameters} ");

            return await getMembers(parameters);
        }

        public async Task<Response<Member>> GetMoreCommunityMembers(string after, string[] fields = null, int limit = 25)
        {
            HttpClient client = getClient();
            Dictionary<string, string[]> requestParameters = new Dictionary<string, string[]>();
            requestParameters.Add(nameof(limit), new string[] { limit.ToString() });
            requestParameters.Add(nameof(after), new string[] { after.ToString() });
            if (fields != null)
            {
                requestParameters.Add(nameof(fields), fields);
            }
            string parameters = generateQueryString(requestParameters);
            _logger.LogInformation($"Request: {this._configuration.CommunityMembersPath}{parameters} ");

            return await getMembers(parameters);
        }

        public async Task<List<Member>> GetAllCommunityMembers(string[] fields = null)
        {
            List<Member> members = new List<Member>();
            int limit = 100;
            Response<Member> response = await GetCommunityMembers(fields, limit);
            bool completed = response.Data == null || response.Data.Count < limit;
            while(!completed)
            {
                string cursorAfter = response.Paging.Cursors.After;
                response = await GetMoreCommunityMembers(cursorAfter, fields, limit);
                completed = response.Data == null || response.Data.Count < limit;
                members.AddRange(response.Data);
            }
            return members;
        }

        private async Task<Response<Member>> getMembers(string parameters)
        {
            HttpClient client = getClient();
            HttpResponseMessage response = await client.GetAsync($"{this._configuration.CommunityMembersPath}{parameters}").ConfigureAwait(false);
            var message = response.Content.ReadAsStringAsync().Result;
            try
            {
                if (response.IsSuccessStatusCode)
                {
                    var jobs = await response.Content.ReadAsStringAsync()
                        .ContinueWith<Response<Member>>(postTask =>
                        {
                            return JsonConvert.DeserializeObject<Response<Member>>(postTask.Result);
                        });
                    return jobs;
                }
            }
            catch (Exception e)
            {
                throw new Exception(message, e);
            }
            return null;
        }

        private HttpClient getClient()
        {
            _logger.LogInformation($"APIBaseUrl: {_configuration.APIBaseUrl} ");
            _logger.LogInformation($"AdminAccessToken: {_configuration.AdminAccessToken} ");
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(_configuration.APIBaseUrl);//new Uri(string.Format("{0}/{1}",this._configuration.APIBaseUrl, this._configuration.CommunityId));
            httpClient.DefaultRequestHeaders.Accept.Clear();
            // Add an Accept header for JSON format.
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _configuration.AdminAccessToken);
            return httpClient;
        }

        private string generateQueryString(Dictionary<string, string[]> parameters)
        {
            if(parameters == null)
            {
                return string.Empty;
            }

            List<string> formattedParameters = new List<string>();
            foreach(string key in parameters.Keys)
            {
                formattedParameters.Add($"{key}={string.Join(",", parameters[key])}");
            }
            return $"?{string.Join("&", formattedParameters)}";
        }

    }
}
