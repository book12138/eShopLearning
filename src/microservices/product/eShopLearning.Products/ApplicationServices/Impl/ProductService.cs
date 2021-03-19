using AutoMapper;
using eShopLearning.Products.Dto;
using eShopLearning.Products.EFCoreRepositories.EFCore;
using eShopLearning.Products.EFCoreRepositories.Entities;
using eShopLearning.Common;
using eShopLearning.Common.Helper;
using eShopLearning.Users.EFCoreRepositories.Repositories;
using Microsoft.EntityFrameworkCore;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.Products.ApplicationServices.Impl
{
    public class ProductService : IProductService
    {
        /// <summary>
        /// sku 仓储
        /// </summary>
        private readonly ISkuRepository _skuRepository;
        /// <summary>
        /// spu 仓储
        /// </summary>
        private readonly ISpuRepository _spuRepository;
        /// <summary>
        /// sku attr 仓储
        /// </summary>
        private readonly ISkuAttrRepository _skuAttrRepository;
        /// <summary>
        /// automapper
        /// </summary>
        private readonly IMapper _mapper;
        /// <summary>
        /// EF Core
        /// </summary>
        private readonly eShopProductDbContext _eShopProductDbContext;
        /// <summary>
        /// es 数据服务
        /// </summary>
        private readonly ISkuEsService _skuEsService;

        /// <summary>
        /// 构造注入
        /// </summary>
        /// <param name="skuRepository"></param>
        /// <param name="spuRepository"></param>
        /// <param name="skuAttrRepository"></param>
        /// <param name="mapper"></param>
        /// <param name="miniShopSkuContext"></param>
        /// <param name="skuEsService"></param>
        public ProductService(
            ISkuRepository skuRepository,
            ISpuRepository spuRepository,
            ISkuAttrRepository skuAttrRepository,
            IMapper mapper,
            eShopProductDbContext @eShopProductDbContext,
            ISkuEsService skuEsService
            )
        {
            _skuRepository = skuRepository;
            _spuRepository = spuRepository;
            _skuAttrRepository = skuAttrRepository;
            _skuEsService = skuEsService;
            _mapper = mapper;
            _eShopProductDbContext = eShopProductDbContext;
        }

        /// <summary>
        /// 添加商品
        /// </summary>
        /// <param name="category"></param>
        /// <param name="skuDtos"></param>
        /// <returns></returns>
        public async Task<(bool isSuccess, string errorMag, IEnumerable<Sku> skus)> AddProduct(string category, IEnumerable<SkuDto> skuDtos)
        {
            if (string.IsNullOrEmpty(category))
                return (false, "类别id不可为空", null);

            if (skuDtos == null || !skuDtos.Any())
                return (false, "没有实际的SKU数据", null);

            foreach (var item in skuDtos.Select(u => u.Title))
                if (await _eShopProductDbContext.Skus.AnyAsync(u => u.Title.Trim() == item))
                    return (false, "该商品已经有过记录", null);

            var spuId = SnowFlakeAlg.GetGuid().ToString(); //_snowflakeIdGenerate.NextId().ToString();

            await _spuRepository.AddAsync(new Spu { Category = category, Id = spuId });
            List<Sku> skuModels = new List<Sku>();
            foreach (var item in skuDtos)
            {
                var skuId = SnowFlakeAlg.GetGuid().ToString();
                var skuModel = _mapper.Map<Sku>(item);
                skuModel.Id = skuId;
                skuModel.SpuId = spuId;
                skuModels.Add(skuModel);

                var skuAttrModels = _mapper.Map<IEnumerable<SkuAttr>>(item.SkuAttrs).Select(u => {
                    u.SkuId = skuId;
                    u.Id = SnowFlakeAlg.GetGuid().ToString();
                    return u;
                });
                await _skuAttrRepository.BatchAddAsync(skuAttrModels);
            }
            await _skuRepository.BatchAddAsync(skuModels);

            await _eShopProductDbContext.SaveChangesAsync();

            return (true, null, skuModels);
        }
    }
}
