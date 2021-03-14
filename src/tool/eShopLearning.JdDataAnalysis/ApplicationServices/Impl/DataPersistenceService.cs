using eShopLearning.JdDataAnalysis.Dto;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopLearning.JdDataAnalysis.ApplicationServices.Impl
{
    public class DataPersistenceService : IDataPersistenceService
    {
        /// <summary>
        /// 日志
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// 构造注入
        /// </summary>
        /// <param name="logger"></param>
        public DataPersistenceService(ILogger<DataPersistenceService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 批量保存抓取到的 SKU 数据
        /// </summary>
        /// <param name="jdSkuDtos"></param>
        /// <returns></returns>
        public Task<bool> BatchSaveSkuData(IEnumerable<JdSkuDto> jdSkuDtos)
        {
            throw new NotImplementedException();
        }
    }
}
