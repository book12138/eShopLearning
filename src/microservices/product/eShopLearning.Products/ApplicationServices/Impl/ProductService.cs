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
using eShopLearning.Products.ViewModel;
using eShopLearning.Common.Extension.LinqExtensions;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using eShopLearning.Products.ApplicationServices.QueryServices;

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
        /// 日志
        /// </summary>
        private readonly ILogger _logger;
        /// <summary>
        /// 商品查询服务
        /// </summary>
        private readonly IProductQueryService _productQueryService;

        /// <summary>
        /// 构造注入
        /// </summary>
        /// <param name="skuRepository"></param>
        /// <param name="spuRepository"></param>
        /// <param name="skuAttrRepository"></param>
        /// <param name="mapper"></param>
        /// <param name="miniShopSkuContext"></param>
        /// <param name="eShopProductDbContext"></param>
        /// <param name="logger"></param>
        /// <param name="productQueryService"></param>
        public ProductService(
            ISkuRepository skuRepository,
            ISpuRepository spuRepository,
            ISkuAttrRepository skuAttrRepository,
            IMapper mapper,
            eShopProductDbContext @eShopProductDbContext,
            ILogger<ProductService> logger,
            IProductQueryService productQueryService
            )
        {
            _skuRepository = skuRepository;
            _spuRepository = spuRepository;
            _skuAttrRepository = skuAttrRepository;
            _mapper = mapper;
            _eShopProductDbContext = eShopProductDbContext;
            _logger = logger;
            _productQueryService = productQueryService;
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
                if (await _productQueryService.ExistSku(item.Trim())) //await _eShopProductDbContext.Skus.AnyAsync(u => u.Title.Trim() == item)
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

        /// <summary>
        /// 获取SKU详情
        /// </summary>
        /// <param name="skuId"></param>
        /// <returns></returns>
        public async Task<ResponseModel<ProductDetailsViewModel>> GetProductDetail(string skuId)
        {
            if (skuId is null or "")
                return ResponseModel<ProductDetailsViewModel>.BuildResponse(PublicStatusCode.Fail, "sku id 不可为空");

            var skuModel = await _productQueryService.QuerySkuAsId(skuId);
                // await _eShopProductDbContext.Skus.FirstOrDefaultAsync(u => u.Status && u.Id == skuId);
            if(skuModel is null)
                return ResponseModel<ProductDetailsViewModel>.BuildResponse(PublicStatusCode.Fail, "此商品不存在或已下架");

            var result = _mapper.Map<ProductDetailsViewModel>(skuModel);
            var targetSkuAttrs = (await _productQueryService.QuerySkuAttrsAsSkuId(skuId)).ToList();
                // await _eShopProductDbContext.SkuAttrs.Where(u => u.Status && u.SkuId == skuId).ToListAsync(); // 找出该sku的所有属性
            if(targetSkuAttrs is null || targetSkuAttrs.Any() is false)
                return ResponseModel<ProductDetailsViewModel>.BuildResponse(PublicStatusCode.Success, result);
            targetSkuAttrs = targetSkuAttrs.OrderBy(u => u.SkuId).ToList();

            result.AttrSkuGroups = new List<ProductDetailsViewModel.AttrSkuGroup>(); // 初始化属性，准备用来存储数据

            // 一次性把该商品的所有同类sku的属性查出来
            //var skuAttrList = await (from s in _eShopProductDbContext.Skus.Where(u => u.Status && u.SpuId == skuModel.SpuId)
            //               join a in _eShopProductDbContext.SkuAttrs on s.Id equals a.SkuId
            //               select a).ToListAsync();
            var skuAttrList = await _productQueryService.QueryAllSkuAttrsAsSpuId(skuModel.SpuId);

            // 遍历该 sku 的每一种类型的属性，然后，找出其他属性类型都相同，但是当前类型里不同属性值的sku，相互之间组合成一个sku组
            for (int i = 0; i < targetSkuAttrs.Count(); i++)
            {
                List<SkuAttr> temp = JsonConvert.DeserializeObject<List<SkuAttr>>(JsonConvert.SerializeObject(skuAttrList));

                // 同组中，保留不同，去掉相同
                // _logger.LogInformation("准备去掉同组属性中，同属性值，但是 不是 需要查询的那个sku");
                // _logger.LogInformation("待处理数据共有{count}条", temp.Count);
                for (int c = temp.Count - 1; c >= 0; c--)
                {
                    var hasRemoveSkuFromSkuAttrList = "";
                    if (
                        temp[c].SkuId == hasRemoveSkuFromSkuAttrList
                            ||
                        (temp[c].Type == targetSkuAttrs[i].Type && temp[c].Name == targetSkuAttrs[i].Name && temp[c].SkuId != skuId)
                        )
                    {
                        hasRemoveSkuFromSkuAttrList = temp[c].SkuId;
                        temp.RemoveAt(c);
                    }
                }
                // _logger.LogInformation("处理完成后，剩余sku属性记录还有{count}条", temp.Count);

                for (int j = 0; j < targetSkuAttrs.Count(); j++)
                {
                    if (i == j)
                        continue;

                    // 遍历除当前上一层循环体当前所指向的分组，对其他所有分组，都剔除掉和查询目标sku属性值所不同的sku
                    _logger.LogInformation("准备去掉其他组属性中，与查询目标sku属性值不同的sku");
                    _logger.LogInformation("待处理数据共有{count}条", temp.Count);
                    _logger.LogInformation("删除属性值不为{name}的sku", targetSkuAttrs[j].Name);
                    var hasRemoveSkuFromSkuAttrList = "";
                    for (int c = temp.Count - 1; c >= 0; c--)
                        if (
                            temp[c].SkuId == hasRemoveSkuFromSkuAttrList 
                            || 
                            (temp[c].Type == targetSkuAttrs[j].Type && temp[c].Name != targetSkuAttrs[j].Name)
                            )
                        {
                            hasRemoveSkuFromSkuAttrList = temp[c].SkuId;
                            temp.RemoveAt(c);
                        }
                    _logger.LogInformation("处理完成后，剩余sku属性记录还有{count}条", temp.Count);
                }

                _logger.LogInformation("该组属性中，筛选出来的结果是{result}", temp.Where(u => u.Type == targetSkuAttrs[i].Type).Select(u => u.Name));
                _logger.LogInformation("该组属性中，另一组属性的结果是{result}", temp.Where(u => u.Type == targetSkuAttrs[1 - i].Type).Select(u => u.Name));
                result.AttrSkuGroups.Add(new ProductDetailsViewModel.AttrSkuGroup
                {
                    Name = targetSkuAttrs[i].Type,
                    AttrSkus = _mapper.Map<IEnumerable<ProductDetailsViewModel.AttrSkuGroup.AttrSku>>(temp.Where(u => u.Type == targetSkuAttrs[i].Type).Distinct(u => u.Name))
                });
            }

            return ResponseModel<ProductDetailsViewModel>.BuildResponse(PublicStatusCode.Success, result);
        }
    }
}
