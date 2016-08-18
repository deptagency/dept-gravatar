using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace D.Models.Common.Interfaces
{
    public interface ICryptographyService
    {
        string CalculateMD5Hash(string input);
    }
}
