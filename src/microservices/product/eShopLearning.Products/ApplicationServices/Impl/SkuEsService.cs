using AutoMapper;
using eShopLearning.Products.Dto;
using eShopLearning.Products.EFCoreRepositories.EFCore;
using eShopLearning.Products.EFCoreRepositories.Entities;
using Microsoft.EntityFrameworkCore;
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
        private readonly eShopProductDbContext _eShopProductDbContext;

        /// <summary>
        /// 构造注入
        /// </summary>
        /// <param name="elasticClient"></param>
        /// <param name="mapper"></param>
        /// <param name="eShopProductDbContext"></param>
        public SkuEsService(IElasticClient elasticClient, IMapper mapper, eShopProductDbContext @eShopProductDbContext)
        {
            _elasticClient = elasticClient;
            _mapper = mapper;
            _eShopProductDbContext = eShopProductDbContext;
        }

        /// <summary>
        /// 将sku数据保存至es
        /// </summary>
        /// <param name="skus"></param>
        /// <returns></returns>
        public virtual async Task SaveSkuData(IEnumerable<Sku> skus)
        {
            foreach (var item in skus)
            {
                var esSkuDto = _mapper.Map<EsSkuDto>(item);
                await _elasticClient.IndexAsync(esSkuDto, u => u.Index("eshopjdproduct").Id(esSkuDto.SkuId));
            }
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

        public async Task SaveAllSkuDataToEsFromDb()
        {
            if (!await _eShopProductDbContext.Skus.AnyAsync())
                return;
             
            return;
            foreach (var item in _eShopProductDbContext.Skus)
            {
                var esSkuDto = _mapper.Map<EsSkuDto>(item);
                await _elasticClient.IndexAsync(esSkuDto, u => u.Index("eshopjdskudata").Id(esSkuDto.SkuId));
            }
        }
    }
}
