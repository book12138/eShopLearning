using AutoMapper;
using eShopLearning.JdDataAnalysis.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopLearning.JdDataAnalysis.ApplicationServices.Impl
{
    public class ProxyIpService : IProxyIpService
    {
        /// <summary>
        /// automapper
        /// </summary>
        private readonly IMapper _mapper;
        /// <summary>
        /// 构造注入
        /// </summary>
        /// <param name="mapper"></param>
        public ProxyIpService(IMapper mapper)
            => _mapper = mapper;

        /// <summary>
        /// 获取一个ip
        /// </summary>
        /// <returns></returns>
        public async Task<IpInfoDto> GetIp()
        {
            //using var channel = GrpcChannel.ForAddress("https://localhost:5891");
            //var client = new IpInfoGrpc.IpInfoGrpcClient(channel);
            //return _mapper.Map<IpInfoDto>(await client.GetAnIpAtRandomAsync(new GetAnIpAtRandomRequest()) ?? new GetAnIpAtRandomReply());
            return null;
        }
    }
}
