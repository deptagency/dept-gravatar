using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace D.Models.Common.Configuration
{
    public class GraphAPIConfiguration
    {
        public string APIBaseUrl { get; set; } = "https://graph.facebook.com";
        public string AdminAccessToken { get; set; }

        public string CommunityMembersPath => $"/{CommunityId}/members";

        public string CommunityId { get; set; }
    }
}
