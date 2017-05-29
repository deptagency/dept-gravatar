using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace D.Models.Google.Interfaces
{
    public interface IGoogleAdminService
    {
        Task<IEnumerable<string>> GetAccountAliases(string email);
    }
}
