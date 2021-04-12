using eShopLearning.Common;
using eShopLearning.WapAggregator.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.WapAggregator.ApplicationServices
{
    public interface IProductService
    {
        /// <summary>
        /// 搜索
        /// </summary>
        /// <param name="keyword">关键字</param>
        /// <param name="page">页码</param>
        /// <param name="size">每页显示的数量</param>
        /// <returns></returns>
        IAsyncEnumerable<ResponseModel<SearchViewModel>> Search(string keyword, int page, int size);
        
    }
}
