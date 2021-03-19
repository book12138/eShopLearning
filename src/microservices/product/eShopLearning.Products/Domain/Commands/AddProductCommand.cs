using eShopLearning.Products.Dto;
using eShopLearning.Products.EFCoreRepositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.Products.Domain.Commands
{
    public class AddProductCommand : Base.DomainCommand
    {
        /// <summary>
        /// 产品所属分类
        /// </summary>
        public string Category { get; set; }
        /// <summary>
        /// 产品sku
        /// </summary>
        public IEnumerable<SkuDto> SkuDtos { get; set; }

        public AddProductCommand(string category, IEnumerable<SkuDto> skuDtos)
        {
            this.Category = category;
            this.SkuDtos = skuDtos;
        }
    }
}
