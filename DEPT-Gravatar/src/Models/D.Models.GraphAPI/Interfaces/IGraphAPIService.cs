using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace D.Models.GraphAPI.Interfaces
{
    public interface IGraphAPIService
    {
        Task<Response<Member>> GetCommunityMembers(string[] fields = null, int offset = 0, int limit = 25);
        Task<List<Member>> GetAllCommunityMembers(string[] fields = null);
    }
}
