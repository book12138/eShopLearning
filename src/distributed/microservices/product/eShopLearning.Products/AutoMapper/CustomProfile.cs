using AutoMapper;
using eShopLearning.Products.Dto;
using eShopLearning.Products.EFCoreRepositories.Entities;
using eShopLearning.Products.gRPC.Protos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.Products.AutoMapper
{
    public class CustomProfile : Profile
    {
        /// <summary>
        /// 配置构造函数，用来创建关系映射
        /// </summary>
        public CustomProfile()
        {
            CreateMap<SkuDto, Sku>()
                .ForMember(dest => dest.RotatePictures, opt => opt.MapFrom(src => string.Join(',', src.RotatePictures)))
                .ForMember(dest => dest.DetailContent, opt => opt.MapFrom(src => string.Join(',', src.DetailContent)));
            CreateMap<SkuAttrDto, SkuAttr>();
            CreateMap<Sku, EsSkuDto>()
                .ForMember(dest => dest.Image, opt => opt.MapFrom(src => SplitStrAsComma(src.RotatePictures).FirstOrDefault() ?? ""))
                .ForMember(dest => dest.SkuId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.SpuId, opt => opt.MapFrom(src => long.Parse(src.SpuId)));
            CreateMap<EsSkuDto, SearchReply>()
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => float.Parse(src.Price.ToString())));
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
