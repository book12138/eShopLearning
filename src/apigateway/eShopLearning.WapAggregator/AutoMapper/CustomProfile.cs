using AutoMapper;
using eShopLearning.WapAggregator.gRPC.Protos;
using eShopLearning.WapAggregator.ViewModel;
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
            CreateMap<SearchReply, SearchViewModel>()
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => decimal.Parse(src.Price.ToString())));
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
