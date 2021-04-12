using AutoMapper;
using eShopLearning.WapAggregator.ApplicationGrpcRemoteServices.Protos;
using eShopLearning.WapAggregator.Dto;
using eShopLearning.WapAggregator.ViewModel;

namespace eShopLearning.Products.AutoMapper
{
    public class CustomProfile : Profile
    {
        /// <summary>
        /// 配置构造函数，用来创建关系映射
        /// </summary>
        public CustomProfile()
        {
            CreateMap<SearchReply, SearchViewModel>()
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => decimal.Parse(src.Price.ToString())));
            CreateMap<GetSkuBasikInfoAsIdReply, SkuBasicInfo>()
                .ForMember(dest => dest.SkuAttrs, opt => opt.MapFrom(src => SplitStrAsComma(src.SkuAttrs ?? "")));
            CreateMap<UserCartQueryResponseDto, UserCartProductDto>();
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
