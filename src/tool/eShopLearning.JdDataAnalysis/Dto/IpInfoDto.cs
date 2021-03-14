using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopLearning.JdDataAnalysis.Dto
{
    public class IpInfoDto
    {
        /// <summary>
        /// ip地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 端口号
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// 地理位置
        /// </summary>
        public string Location { get; set; }
        /// <summary>
        /// 运营商
        /// </summary>
        public string Operator { get; set; }
    }
}
