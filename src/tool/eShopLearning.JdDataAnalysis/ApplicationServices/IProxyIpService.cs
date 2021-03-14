using eShopLearning.JdDataAnalysis.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopLearning.JdDataAnalysis.ApplicationServices
{
    public interface IProxyIpService
    {
        /// <summary>
        /// 获取一个ip
        /// </summary>
        /// <returns></returns>
        Task<IpInfoDto> GetIp();
    }
}
