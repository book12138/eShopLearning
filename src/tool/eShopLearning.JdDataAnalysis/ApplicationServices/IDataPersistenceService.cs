using eShopLearning.JdDataAnalysis.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopLearning.JdDataAnalysis.ApplicationServices
{
    public interface IDataPersistenceService
    {
        /// <summary>
        /// 批量保存抓取到的 SKU 数据
        /// </summary>
        /// <param name="jdSkuDtos"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        Task<bool> BatchSaveSkuData(IEnumerable<JdSkuDto> jdSkuDtos, string category);
    }
}
