using PharmaMoov.Models;
using PharmaMoov.Models.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PharmaMoov.API.DataAccessLayer.Interfaces
{
    public interface IConfigRepository
    {
        APIResponse GetAllConfigurations();
        APIResponse UpdateOrderConfig(List<OrderConfiguration> _configs);
    }
}
