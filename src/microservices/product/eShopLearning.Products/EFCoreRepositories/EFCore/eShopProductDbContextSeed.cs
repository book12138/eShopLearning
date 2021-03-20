using eShopLearning.Products.EFCoreRepositories.Entities;
using eShopLearning.Common.Extension;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using eShopLearning.Products.ApplicationServices;

namespace eShopLearning.Products.EFCoreRepositories.EFCore
{
    /// <summary>
    /// EF Core 种子数据迁移
    /// </summary>
    public class eShopProductDbContextSeed
    {
        /// <summary>
        /// 系统配置读取
        /// </summary>
        private readonly IConfiguration _configuration;
        /// <summary>
        /// sku es
        /// </summary>
        private readonly ISkuEsService _skuEsService;

        /// <summary>
        /// 构造注入
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="skuEsService"></param>
        public eShopProductDbContextSeed(IConfiguration configuration, ISkuEsService skuEsService)
        {
            _configuration = configuration;
            _skuEsService = skuEsService;
        }

        /// <summary>
        /// 迁移种子数据
        /// </summary>
        /// <param name="context">ef core db context</param>
        /// <param name="env">主机环境</param>
        /// <param name="logger">日志</param>
        /// <param name="retry">重试次数</param>
        /// <returns></returns>
        public async Task SeedDataMigrationAsync(eShopProductDbContext context, IWebHostEnvironment env, ILogger<eShopProductDbContextSeed> logger, int? retry = 0)
        {
            int retryForAvaiability = retry.Value;

            try
            {
                var contentRootPath = env.ContentRootPath;
                var webroot = env.WebRootPath;

                if (!context.Categories.Any())
                {
                    // 根据系统配置而决定是迁移代码中定义的种子数据，还是迁移csv文件中的
                    context.Categories.AddRange(
                        (bool.TryParse(_configuration["AppSettings:UseCustomizationSeedData"], out bool parseResult) ? parseResult : false)
                        ? GetCategoryFromFile(contentRootPath, logger) // 加载文件中的数据
                        : new List<Category>()); // 加载代码定义的数据

                    await context.SaveChangesAsync();
                }

                if (!context.Spus.Any())
                {
                    // 根据系统配置而决定是迁移代码中定义的种子数据，还是迁移csv文件中的
                    context.Spus.AddRange(
                        (bool.TryParse(_configuration["AppSettings:UseCustomizationSeedData"], out bool parseResult) ? parseResult : false)
                        ? GetSpuFromFile(contentRootPath, logger) // 加载文件中的数据
                        : new List<Spu>()); // 加载代码定义的数据

                    await context.SaveChangesAsync();
                }

                if (!context.Skus.Any())
                {
                    // 根据系统配置而决定是迁移代码中定义的种子数据，还是迁移csv文件中的
                    context.Skus.AddRange(
                        (bool.TryParse(_configuration["AppSettings:UseCustomizationSeedData"], out bool parseResult) ? parseResult : false)
                        ? await GetSkuFromFile(contentRootPath, logger) // 加载文件中的数据
                        : new List<Sku>()); // 加载代码定义的数据

                    await context.SaveChangesAsync();
                }

                if (!context.SkuAttrs.Any())
                {
                    // 根据系统配置而决定是迁移代码中定义的种子数据，还是迁移csv文件中的
                    context.SkuAttrs.AddRange(
                        (bool.TryParse(_configuration["AppSettings:UseCustomizationSeedData"], out bool parseResult) ? parseResult : false)
                        ? GetSkuAttrFromFile(contentRootPath, logger) // 加载文件中的数据
                        : new List<SkuAttr>()); // 加载代码定义的数据

                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                if (retryForAvaiability < 10)
                {
                    retryForAvaiability++;

                    logger.LogError(ex, "当针对 {DbContextName} 进行迁移时发生错误", nameof(eShopProductDbContext));

                    await SeedDataMigrationAsync(context, env, logger, retryForAvaiability);
                }
            }
        }

        #region category entity
        /// <summary>
        /// 加载迁移文件中的数据
        /// </summary>
        /// <param name="contentRootPath"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        private IEnumerable<Category> GetCategoryFromFile(string contentRootPath, ILogger logger)
        {
            string csvFile = Path.Combine(contentRootPath, "EFCoreRepositories", "EFCore", "spucategoryseeddata.csv"); // 迁移文件路径

            if (!File.Exists(csvFile))
                return new List<Category>();

            string[] csvheaders;
            try
            {
                string[] requiredHeaders = { "Id","ParentId", "Name", "Image", "Order" };
                csvheaders = GetHeaders(requiredHeaders, csvFile);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "csv文件读取发生错误，具体错误信息为: {Message}", ex.Message);

                return new List<Category>();
            }

            List<Category> categories = File.ReadAllLines(csvFile)
                        .Skip(1) // skip header column
                        .Select(row => Regex.Split(row, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)"))
                        .SelectTry(column => CreateCategory(column, csvheaders))
                        .OnCaughtException(ex => { logger.LogError(ex, "csv文件读取发生错误，具体错误信息为: {Message}", ex.Message); return null; })
                        .Where(x => x != null)
                        .ToList();

            return categories;
        }

        /// <summary>
        /// 创建 category 对象
        /// </summary>
        /// <param name="column"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        private Category CreateCategory(string[] column, string[] headers)
        {
            if (column.Count() != headers.Count())
                throw new Exception($"csv文件表头与数据的列数不一致");

            var category = new Category
            {
                Id = column[Array.IndexOf(headers, "Id")].Trim('"').Trim(),
                ParentId = column[Array.IndexOf(headers, "ParentId")].Trim('"').Trim(),
                Name = column[Array.IndexOf(headers, "Name")].Trim('"').Trim(),
                Image = column[Array.IndexOf(headers, "Image")].Trim('"').Trim(),
                Order = int.TryParse(column[Array.IndexOf(headers, "Order")].Trim('"').Trim(), out int orderParseResult) ? orderParseResult : 0
            };

            return category;
        }
        #endregion

        #region spu entity
        /// <summary>
        /// 加载迁移文件中的数据
        /// </summary>
        /// <param name="contentRootPath"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        private IEnumerable<Spu> GetSpuFromFile(string contentRootPath, ILogger logger)
        {
            string csvFile = Path.Combine(contentRootPath, "EFCoreRepositories", "EFCore", "spuseeddata.csv"); // 迁移文件路径

            if (!File.Exists(csvFile))
                return new List<Spu>();

            string[] csvheaders;
            try
            {
                string[] requiredHeaders = { "Id", "Category" };
                csvheaders = GetHeaders(requiredHeaders, csvFile);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "csv文件读取发生错误，具体错误信息为: {Message}", ex.Message);

                return new List<Spu>();
            }

            List<Spu> spus = File.ReadAllLines(csvFile)
                        .Skip(1) // skip header column
                        .Select(row => Regex.Split(row, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)"))
                        .SelectTry(column => CreateSpu(column, csvheaders))
                        .OnCaughtException(ex => { logger.LogError(ex, "csv文件读取发生错误，具体错误信息为: {Message}", ex.Message); return null; })
                        .Where(x => x != null)
                        .ToList();

            return spus;
        }

        /// <summary>
        /// 创建 spu 对象
        /// </summary>
        /// <param name="column"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        private Spu CreateSpu(string[] column, string[] headers)
        {
            if (column.Count() != headers.Count())
                throw new Exception($"csv文件表头与数据的列数不一致");

            var spu = new Spu
            {
                Category = column[Array.IndexOf(headers, "Category")].Trim('"').Trim(),
                Id = column[Array.IndexOf(headers, "Id")].Trim('"').Trim()
            };

            return spu;
        }
        #endregion

        #region sku entity
        /// <summary>
        /// 加载迁移文件中的数据
        /// </summary>
        /// <param name="contentRootPath"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        private async Task<IEnumerable<Sku>> GetSkuFromFile(string contentRootPath, ILogger logger)
        {
            string csvFile = Path.Combine(contentRootPath, "EFCoreRepositories", "EFCore", "skuseeddata.csv"); // 迁移文件路径

            if (!File.Exists(csvFile))
                return new List<Sku>();

            string[] csvheaders;
            try
            {
                string[] requiredHeaders = { "Id", "Title", "SpuId", "RotatePictures", "OriginalPrice", "Price", "Inventory", "DetailContent"  };
                csvheaders = GetHeaders(requiredHeaders, csvFile);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "csv文件读取发生错误，具体错误信息为: {Message}", ex.Message);

                return new List<Sku>();
            }

            List<Sku> skus = File.ReadAllLines(csvFile)
                        .Skip(1) // skip header column
                        .Select(row => Regex.Split(row, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)"))
                        .SelectTry(column => CreateSku(column, csvheaders))
                        .OnCaughtException(ex => { logger.LogError(ex, "csv文件读取发生错误，具体错误信息为: {Message}", ex.Message); return null; })
                        .Where(x => x != null)
                        .ToList();
            await  _skuEsService.SaveSkuData(skus);
            return skus;
        }

        /// <summary>
        /// 创建 sku 对象
        /// </summary>
        /// <param name="column"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        private Sku CreateSku(string[] column, string[] headers)
        {
            if (column.Count() != headers.Count())
                throw new Exception($"csv文件表头与数据的列数不一致");

            var sku = new Sku
            {
                DetailContent = column[Array.IndexOf(headers, "DetailContent")]?.Trim('"')?.Trim(),
                Inventory = int.TryParse(column[Array.IndexOf(headers, "Inventory")]?.Trim('"')?.Trim(), out int inventoryParseResult) ? inventoryParseResult : 0,
                Price = decimal.TryParse(column[Array.IndexOf(headers, "Price")]?.Trim('"')?.Trim(), out decimal priceParseResult) ? priceParseResult : 0,
                OriginalPrice = string.IsNullOrEmpty(column[Array.IndexOf(headers, "OriginalPrice")]?.Trim('"')?.Trim()) ?
                    null :
                    decimal.TryParse(column[Array.IndexOf(headers, "OriginalPrice")]?.Trim('"')?.Trim(), out decimal originalPriceParseResult) ? originalPriceParseResult : 0,
                RotatePictures = column[Array.IndexOf(headers, "RotatePictures")]?.Trim('"')?.Trim(),
                SpuId = column[Array.IndexOf(headers, "SpuId")]?.Trim('"')?.Trim(),
                Title = column[Array.IndexOf(headers, "Title")]?.Trim('"')?.Trim(),
                Id = column[Array.IndexOf(headers, "Id")]?.Trim('"')?.Trim()
            };
            
            return sku;
        }
        #endregion

        #region skuattr entity
        /// <summary>
        /// 加载迁移文件中的数据
        /// </summary>
        /// <param name="contentRootPath"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        private IEnumerable<SkuAttr> GetSkuAttrFromFile(string contentRootPath, ILogger logger)
        {
            string csvFile = Path.Combine(contentRootPath, "EFCoreRepositories", "EFCore", "skuattrseeddata.csv"); // 迁移文件路径

            if (!File.Exists(csvFile))
                return new List<SkuAttr>();

            string[] csvheaders;
            try
            {
                string[] requiredHeaders = { "Id","Name", "SkuId", "Image", "Type" };
                csvheaders = GetHeaders(requiredHeaders, csvFile);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "csv文件读取发生错误，具体错误信息为: {Message}", ex.Message);

                return new List<SkuAttr>();
            }

            List<SkuAttr> skuAttrs = File.ReadAllLines(csvFile)
                        .Skip(1) // skip header column
                        .Select(row => Regex.Split(row, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)"))
                        .SelectTry(column => CreateSkuAttr(column, csvheaders))
                        .OnCaughtException(ex => { logger.LogError(ex, "csv文件读取发生错误，具体错误信息为: {Message}", ex.Message); return null; })
                        .Where(x => x != null)
                        .ToList();

            return skuAttrs;
        }

        /// <summary>
        /// 创建 skuattr 对象
        /// </summary>
        /// <param name="column"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        private SkuAttr CreateSkuAttr(string[] column, string[] headers)
        {
            if (column.Count() != headers.Count())
                throw new Exception($"csv文件表头与数据的列数不一致");

            var skuAttr = new SkuAttr
            {
                Name = column[Array.IndexOf(headers, "Name")]?.Trim('"')?.Trim(),
                SkuId = column[Array.IndexOf(headers, "SkuId")]?.Trim('"')?.Trim(),
                Image = column[Array.IndexOf(headers, "Image")]?.Trim('"')?.Trim(),
                Type = column[Array.IndexOf(headers, "Type")]?.Trim('"')?.Trim(),
                Id = column[Array.IndexOf(headers, "Id")]?.Trim('"')?.Trim()
            };

            return skuAttr;
        }
        #endregion

        /// <summary>
        /// 解析 csv 文件中表头
        /// </summary>
        /// <param name="requiredHeaders"></param>
        /// <param name="csvfile"></param>
        /// <returns></returns>
        static string[] GetHeaders(string[] requiredHeaders, string csvfile)
        {
            string[] csvheaders = File.ReadLines(csvfile).First().Split(','); // ToLowerInvariant()

            if (csvheaders.Count() != requiredHeaders.Count())
            {
                throw new Exception($"csv文件读取后得到的表头数目不是 '{ requiredHeaders.Count()}'，而是 '{csvheaders.Count()}'");
            }

            foreach (var requiredHeader in requiredHeaders)
            {
                if (!csvheaders.Contains(requiredHeader))
                {
                    throw new Exception($"csv文件的表头 '{requiredHeader}' 不存在");
                }
            }

            return csvheaders;
        }
    }
}
