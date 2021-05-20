using Elasticsearch.Net;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.Products.ApplicationServices.Base
{
    public class BaseQueryService
    {
        /// <summary>
        /// 系统配置读取
        /// </summary>
        protected readonly IConfiguration _configuration;

        public BaseQueryService(IConfiguration configuration) => _configuration = configuration;

        /// <summary>
        /// mysql连接对象工厂
        /// </summary>
        /// <returns></returns>
        public DbConnection MysqlConnectionFactory()
        {
            return new MySqlConnection(_configuration["MysqlConnStr"]);
        }
    }
}
