using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace D.Models.GraphAPI.Interfaces
{
    public interface IGraphAPIService
    {
        Task<Response<Member>> GetCommunityMembers(string[] fields = null, int limit = 25);
        Task<List<Member>> GetAllCommunityMembers(string[] fields = null);
        Task<Response<Member>> GetMoreCommunityMembers(string cursorNext, string[] fields = null, int limit = 25);
    }
}
