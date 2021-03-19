using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopLearning.JdDataAnalysis.ApplicationServices
{
    public interface IProductDataGrabService
    {
        /// <summary>
        /// 从搜索页面下手开始抓取
        /// </summary>        
        /// <param name="keyword">关键字</param>
        /// <param name="category">类别id，保存数据使用</param>
        /// <param name="maxPageCount">最大爬取多少页</param>
        /// <param name="firstPage">起始页数，从第几页开始爬</param>
        /// <returns></returns>
        Task GrabDataFromSearchPage(string keyword, string category, int maxPageCount, int firstPage = 1);
    }
}
