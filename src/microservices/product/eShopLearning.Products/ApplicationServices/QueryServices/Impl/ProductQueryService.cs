using Dapper;
using eShopLearning.Products.ApplicationServices.Base;
using eShopLearning.Products.EFCoreRepositories.EFCore;
using eShopLearning.Products.EFCoreRepositories.Entities;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace eShopLearning.Products.ApplicationServices.QueryServices.Impl
{
    /// <summary>
    /// 商品查询服务
    /// </summary>
    public class ProductQueryService : BaseQueryService, IProductQueryService
    {
        #region 拼接表字段，变成这种格式：`table1`.`field1`,`table1`.`field2`,`table1`.`field3`,
        /// <summary>
        /// sku 表所有字段
        /// </summary>
        private static readonly string _skuAllFields = string.Join(',', typeof(Sku).GetRuntimeProperties().Select(u => $"`{nameof(eShopProductDbContext.Skus)}`.`{u.Name}`"));
        /// <summary>
        /// sku attr属性表所有字段
        /// </summary>
        private static readonly string _skuAttrAllFields = string.Join(',', typeof(SkuAttr).GetRuntimeProperties().Select(u => $"`{nameof(eShopProductDbContext.SkuAttrs)}`.`{u.Name}`"));
        #endregion

        /// <summary>
        /// 构造注入
        /// </summary>
        /// <param name="configuration"></param>
        public ProductQueryService(IConfiguration configuration)
            : base(configuration)
        { }

        /// <summary>
        /// 根据sku id查询sku记录
        /// </summary>
        /// <param name="skuId">sku id</param>
        /// <param name="onlyUndeletedRecord">指定是否只查询未被删除的记录</param>
        /// <returns></returns>
        public async Task<Sku> QuerySkuAsId(string skuId, bool onlyUndeletedRecord = true)
        {
            /* 拼接 sql */
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("select {0} from `{1}` where `{2}` = '{3}'", 
                _skuAllFields, 
                nameof(eShopProductDbContext.Skus),
                nameof(Sku.Id),
                skuId);

            if (onlyUndeletedRecord)
                stringBuilder.AppendFormat(" and `{0}` = '{1}'", nameof(Sku.Status), 1);

            using(var conn = base.MysqlConnectionFactory())
                return await conn.QueryFirstOrDefaultAsync<Sku>(stringBuilder.ToString());
        }

        /// <summary>
        /// 根据 sku id 查询该sku的属性
        /// </summary>
        /// <param name="skuId">sku id</param>
        /// <param name="onlyUndeletedRecord">指定是否只查询未被删除的记录</param>
        /// <returns></returns>
        public async Task<IEnumerable<SkuAttr>> QuerySkuAttrsAsSkuId(string skuId, bool onlyUndeletedRecord = true)
        {
            /* 拼接 sql */
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("select {0} from `{1}` where `{2}` = '{3}'",
                _skuAttrAllFields,
                nameof(eShopProductDbContext.SkuAttrs),
                nameof(SkuAttr.SkuId),
                skuId);

            if (onlyUndeletedRecord)
                stringBuilder.AppendFormat(" and `{0}` = '{1}'", nameof(Sku.Status), 1);

            using (var conn = base.MysqlConnectionFactory())
                return await conn.QueryAsync<SkuAttr>(stringBuilder.ToString());
        }

        /// <summary>
        /// 根据 spu id 查询该spu下所有sku的属性
        /// </summary>
        /// <param name="skuId">sku id</param>
        /// <param name="onlyUndeletedRecord">指定是否只查询未被删除的记录</param>
        /// <returns></returns>
        public async Task<IEnumerable<SkuAttr>> QueryAllSkuAttrsAsSpuId(string spuId, bool onlyUndeletedRecord = true)
        {
            StringBuilder stringBuilder = new StringBuilder();
            // select SkuAttrs.Id,SkuAttrs.`Name`,SkuAttrs.Type from SkuAttrs inner join Skus on Skus.Id = SkuAttrs.SkuId where Skus.SpuId = '6798634499361124352'
            stringBuilder.AppendFormat("select {0} from SkuAttrs inner join Skus on Skus.Id = SkuAttrs.SkuId where Skus.SpuId = '{1}'",
                _skuAttrAllFields, spuId);

            if (onlyUndeletedRecord)
                stringBuilder.Append(" and `Skus`.`Status` = 1 and `SkuAttrs`.`Status` = 1");

            using (var conn = base.MysqlConnectionFactory())
                return await conn.QueryAsync<SkuAttr>(stringBuilder.ToString());
        }

        /// <summary>
        /// 检查是否存在此商品
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public async Task<bool> ExistSku(string title)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("select count(1) from `{0}` where `{1}` = '{2}'", nameof(eShopProductDbContext.Skus), nameof(Sku.Title), title);
            using (var conn = base.MysqlConnectionFactory())
            {
                var temp = await conn.QueryFirstAsync<int>(stringBuilder.ToString());
                return temp == 0 ? false : true;
            }
        }
    }
}
