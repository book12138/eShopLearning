using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClientDemo.ApplicationServices
{
    public interface ITestService
    {
        /// <summary>
        /// 检查一下能否和api网关进行正常通信
        /// </summary>
        /// <returns></returns>
        Task<string> TestApiGatewayService();
        /// <summary>
        /// 请求api service
        /// </summary>
        /// <returns></returns>
        Task<string> TestApiService();
    }
}
