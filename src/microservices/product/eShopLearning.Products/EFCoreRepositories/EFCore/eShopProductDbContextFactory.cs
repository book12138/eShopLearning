using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace eShopLearning.Products.EFCoreRepositories.EFCore
{
    /// <summary>
    /// 创建 ApplicationUserDbContext 的小作坊
    /// 迁移的时候使用
    /// </summary>
    public class eShopProductDbContextFactory : IDesignTimeDbContextFactory<eShopProductDbContext>
    {
        public eShopProductDbContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
               .SetBasePath(Path.Combine(Directory.GetCurrentDirectory()))
               .AddJsonFile("appsettings.json")
               .AddEnvironmentVariables()
               .Build();

            var optionsBuilder = new DbContextOptionsBuilder<eShopProductDbContext>();

            optionsBuilder.UseMySql(config["MysqlConnStr"],
                     ServerVersion.AutoDetect(config["MysqlConnStr"]),
                     mySqlOptionsAction: options =>
                     {
                         options.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
                         options.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                     });

            return new eShopProductDbContext(optionsBuilder.Options);
        }
    }
}
