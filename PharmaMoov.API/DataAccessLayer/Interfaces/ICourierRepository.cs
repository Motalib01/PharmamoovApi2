using PharmaMoov.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PharmaMoov.API.DataAccessLayer.Interfaces
{
    public interface ICourierRepository
    {
        APIResponse GetCouriers(string Authorization, int CourierID);
    }
}
