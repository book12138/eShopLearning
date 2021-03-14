using eShopLearning.Carts.EFCoreRepositories.EFCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace eShopLearning.Users.EFCoreRepositories.EFCore
{
    /// <summary>
    /// EF Core 种子数据迁移
    /// </summary>
    public class eShopCartDbContextSeed
    {
        /// <summary>
        /// 系统配置读取
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// 构造注入
        /// </summary>
        /// <param name="configuration"></param>
        public eShopCartDbContextSeed(IConfiguration configuration)
            => _configuration = configuration;

        /// <summary>
        /// 迁移种子数据
        /// </summary>
        /// <param name="context">ef core db context</param>
        /// <param name="env">主机环境</param>
        /// <param name="logger">日志</param>
        /// <param name="retry">重试次数</param>
        /// <returns></returns>
        public async Task SeedDataMigrationAsync(eShopCartDbContext context, IWebHostEnvironment env, ILogger<eShopCartDbContextSeed> logger, int? retry = 0)
        {
            int retryForAvaiability = retry.Value;

            try
            {
                var contentRootPath = env.ContentRootPath;
                var webroot = env.WebRootPath;
            }
            catch (Exception ex)
            {
                if (retryForAvaiability < 10)
                {
                    retryForAvaiability++;

                    logger.LogError(ex, "当针对 {DbContextName} 进行迁移时发生错误", nameof(eShopCartDbContext));

                    await SeedDataMigrationAsync(context, env, logger, retryForAvaiability);
                }
            }
        }
    }
}
