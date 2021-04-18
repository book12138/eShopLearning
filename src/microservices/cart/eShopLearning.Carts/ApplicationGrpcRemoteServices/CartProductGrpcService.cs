using AutoMapper;
using eShopLearning.Carts.ApplicationGrpcRemoteServices.Protos;
using eShopLearning.Carts.ApplicationServices;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using NConsul;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.Carts.ApplicationGrpcRemoteServices
{
    public class CartProductGrpcService : CartProductGrpc.CartProductGrpcBase
    {
        /// <summary>
        /// 日志
        /// </summary>
        private readonly ILogger _logger;
        /// <summary>
        /// automapper
        /// </summary>
        private readonly IMapper _mapper;
        /// <summary>
        /// 购物车应用服务
        /// </summary>
        private readonly ICartService _cartService;

        /// <summary>
        /// 构造注入
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="mapper"></param>
        /// <param name="cartService"></param>
        public CartProductGrpcService(
            ILogger<CartProductGrpcService> logger,
            IMapper mapper,
            ICartService cartService
            )
        {
            _logger = logger;
            _mapper = mapper;
            _cartService = cartService;
        }

        /// <summary>
        /// 查询用户购物车中所有商品
        /// </summary>
        /// <param name="request"></param>
        /// <param name="responseStream"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task GetUserCartAllProduct(GetUserCartAllProductRequest request, IServerStreamWriter<GetUserCartAllProductReply> responseStream, ServerCallContext context)
        {
            _logger.LogInformation("购物车grpc服务收到请求，需要查询用户id为{id}的购物车中的商品", request.UserId);
            var temp = await _cartService.GetUserCartAllProduct(long.TryParse(request.UserId, out long parseResult) ? parseResult : 0);
            _logger.LogInformation("用户{id}的购物车中共有{count}条记录", request.UserId, temp.Count());
            foreach (var item in temp)
                await responseStream.WriteAsync(_mapper.Map<GetUserCartAllProductReply>(item));
        }

        /// <summary>
        /// 查询用户购物车中的商品
        /// </summary>
        /// <param name="request"></param>
        /// <param name="responseStream"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task GetUserCartProduct(GetUserCartProductRequest request, IServerStreamWriter<GetUserCartProductReply> responseStream, ServerCallContext context)
        {
            _logger.LogInformation("购物车grpc服务收到请求，需要查询用户id为{id}的购物车中的第{page}页的商品", request.UserId, request.Page);
            var temp = await _cartService.GetUserCartProduct(
                long.TryParse(request.UserId, out long parseResult) ? parseResult : 0,
                request.Page,
                request.Size);

            _logger.LogInformation("用户{id}的第{page}页购物车数据中共查询到{count}条数据", request.UserId, request.Page, temp.Count());
            foreach (var item in temp)
                await responseStream.WriteAsync(_mapper.Map<GetUserCartProductReply>(item));
        }
    }
}
