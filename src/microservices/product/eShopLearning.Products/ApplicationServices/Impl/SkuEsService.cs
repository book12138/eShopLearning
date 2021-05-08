using AutoMapper;
using eShopLearning.Products.Dto;
using eShopLearning.Products.EFCoreRepositories.EFCore;
using eShopLearning.Products.EFCoreRepositories.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nest;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.Products.ApplicationServices.Impl
{
    public class SkuEsService : ISkuEsService
    {
        /// <summary>
        /// es
        /// </summary>
        private readonly IElasticClient _elasticClient;
        /// <summary>
        /// automapper
        /// </summary>
        private readonly IMapper _mapper;
        /// <summary>
        /// ef core
        /// </summary>
        private readonly eShopProductDbContext _eShopProductDbContext;
        /// <summary>
        /// 日志
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// 构造注入
        /// </summary>
        /// <param name="elasticClient"></param>
        /// <param name="mapper"></param>
        /// <param name="eShopProductDbContext"></param>
        /// <param name="logger"></param>
        public SkuEsService(
            IElasticClient elasticClient, 
            IMapper mapper, 
            eShopProductDbContext @eShopProductDbContext,
            ILogger<SkuEsService> logger
            )
        {
            _elasticClient = elasticClient;
            _mapper = mapper;
            _eShopProductDbContext = eShopProductDbContext;
            _logger = logger;
        }

        /// <summary>
        /// 将sku数据保存至es
        /// </summary>
        /// <param name="skus"></param>
        /// <returns></returns>
        public virtual async Task SaveSkuData(IEnumerable<Sku> skus)
        {
            if(skus is null || skus.Any() is false)
            {
                _logger.LogWarning("本来打算存储到es的sku数据被检查为null或empty，导致无法进行下一步保存计划");
                return;
            }    
            foreach (var item in skus)
            {                
                var esSkuDto = _mapper.Map<EsSkuDto>(item);
                _logger.LogInformation("准备持久化一条sku数据到 es 中，该Sku的id为{id}", esSkuDto.SkuId);
                await _elasticClient.IndexAsync(esSkuDto, u => u.Index("eshopjdskudata").Id(esSkuDto.SkuId));
            }
        }

        /// <summary>
        /// 将sku数据保存至es
        /// </summary>
        /// <param name="sku"></param>
        /// <returns></returns>
        public virtual async Task SaveSkuData(Sku sku)
        {
            if (sku is null || sku.Id is null or "")
            {
                _logger.LogWarning("本来打算存储到es的sku数据被检查为null或empty，导致无法进行下一步保存计划");
                return;
            }

            var esSkuDto = _mapper.Map<EsSkuDto>(sku);
            _logger.LogInformation("准备持久化一条sku数据到 es 中，该Sku的id为{id}", esSkuDto.SkuId);
            await _elasticClient.IndexAsync(esSkuDto, u => u.Index("eshopjdskudata").Id(esSkuDto.SkuId));
        }

        /// <summary>
        /// 搜索
        /// </summary>
        /// <param name="keyword">关键字</param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public async Task<IEnumerable<EsSkuDto>> Search(string keyword, int page, int size)
        {
            _logger.LogInformation("本次搜索的关键字词是：{keyword}", keyword);
            if (string.IsNullOrEmpty(keyword))
                return new List<EsSkuDto>();

            var searchResponse = await _elasticClient.SearchAsync<EsSkuDto>(u =>
                u.Index("eshopjdskudata")
                .Query(q => q.Match(m => m.Field(f => f.Title).Query(keyword)))
                .Collapse(c => c.Field(f => f.SpuId))
                .Skip(page - 1)
                .Size(size)
                );

            return searchResponse.Documents.ToList();
        }

        /// <summary>
        /// 将数据库中的所有sku 数据存储到es中
        /// </summary>
        /// <returns></returns>
        public async Task SaveAllSkuDataToEsFromDb()
        {
            if (!await _eShopProductDbContext.Skus.AnyAsync())
                return;
             
            foreach (var item in _eShopProductDbContext.Skus)
            {
                var esSkuDto = _mapper.Map<EsSkuDto>(item);
                await _elasticClient.IndexAsync(esSkuDto, u => u.Index("eshopjdskudata").Id(esSkuDto.SkuId));
            }
        }
    }
}
