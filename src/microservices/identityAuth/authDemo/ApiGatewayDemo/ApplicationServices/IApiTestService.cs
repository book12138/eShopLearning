using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiGatewayDemo.ApplicationServices
{
    public interface IApiTestService
    {
        Task<string> GetValue();
    }
}
