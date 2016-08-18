using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace D.Models.GraphAPI
{
    public class Member
    {
        public string Id { get; set; }
        [JsonProperty("first_name")]
        public string FirstName { get; set; }
        [JsonProperty("last_name")]
        public string LastName { get; set; }
        public string Email { get; set; }
        public Picture Picture { get; set; }
        public string Link { get; set; }
        public string Locale { get; set; }
        public string Name { get; set; }
        [JsonProperty("name_format")]
        public string NameFormat { get; set; }
        [JsonProperty("updated_time")]
        public string UpdatedTime { get; set; }

    }
}
