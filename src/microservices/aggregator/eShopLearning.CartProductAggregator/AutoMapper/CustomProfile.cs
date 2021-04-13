using AutoMapper;
using eShopLearning.CartProductAggregator.ApplicationGrpcRemoteServices.Protos;
using eShopLearning.CartProductAggregator.Dto;

namespace eShopLearning.CartProductAggregator.AutoMapper
{
    public class CustomProfile : Profile
    {
        /// <summary>
        /// 配置构造函数，用来创建关系映射
        /// </summary>
        public CustomProfile()
        {
            CreateMap<GetUserCartAllProductReply, UserCartProductDto>();
            CreateMap<GetUserCartProductReply, UserCartProductDto>();
            CreateMap<GetSkuBasikInfoAsIdReply, SkuBasicInfo>()
                .ForMember(dest => dest.SkuAttrs, opt => opt.MapFrom(src => SplitStrAsComma(src.SkuAttrs ?? "")));
        }

        /// <summary>
        /// 按照逗号分割字符串成为数组
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static string[] SplitStrAsComma(string target)
        {
            return target?.Trim().Trim(',').Split(',') ?? new string[0];
        }
    }
}
