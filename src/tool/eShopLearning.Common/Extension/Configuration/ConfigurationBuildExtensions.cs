using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopLearning.Common.Extension.Configuration
{
    public static class ConfigurationBuildExtensions
    {
        /// <summary>
        /// 尝试去寻找指定的appsettings.json文件，当发现有的时候，加入到ConfigurationBuilder中
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="path"></param>
        /// <param name="optional"></param>
        /// <param name="reloadOnChange"></param>
        /// <returns></returns>
        public static IConfigurationBuilder TryAddJsonFile(this IConfigurationBuilder builder, string path, bool optional, bool reloadOnChange)
        {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), path);
            if (File.Exists(filePath))
                builder = builder.AddJsonFile(path, optional, reloadOnChange);

            return builder;
        }
    }
}
