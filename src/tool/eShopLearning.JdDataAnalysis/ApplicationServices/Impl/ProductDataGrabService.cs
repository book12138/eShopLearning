using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using eShopLearning.JdDataAnalysis.Dto;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace eShopLearning.JdDataAnalysis.ApplicationServices.Impl
{
    public class ProductDataGrabService : IProductDataGrabService
    {
        /// <summary>
        /// 日志
        /// </summary>
        private readonly ILogger _logger;
        /// <summary>
        /// 数据持久化
        /// </summary>
        private readonly IDataPersistenceService _dataPersistenceService;
        /// <summary>
        /// 代理IP服务
        /// </summary>
        private readonly IProxyIpService _proxyIpService;

        /// <summary>
        /// 构造注入
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="dataPersistenceService"></param>
        /// <param name="proxyIpService"></param>
        public ProductDataGrabService(
            ILogger<ProductDataGrabService> logger,
            IDataPersistenceService dataPersistenceService,
            IProxyIpService proxyIpService
            )
        {
            _logger = logger;
            _dataPersistenceService = dataPersistenceService;
            _proxyIpService = proxyIpService;
        }

        /// <summary>
        /// 从搜索页面下手开始抓取
        /// </summary>        
        /// <param name="keyword">关键字</param>
        /// <param name="maxPageCount">最大爬取多少页</param>
        /// <param name="firstPage">起始页数，从第几页开始爬</param>
        /// <returns></returns>
        public async Task GrabDataFromSearchPage(string keyword, int maxPageCount, int firstPage = 1)
        {
            List<JdSkuDto> jdSkuDtos = new List<JdSkuDto>(); // 最终爬取来的数据会填充进去

            if (firstPage % 2 != 1) // JD搜索页是页码都是奇数，２为递增量
                firstPage += 1;

            int page = 0;
            while (page++ < maxPageCount)
            {
                var searchPageHtml = await SendRequest(
                    $"https://search.jd.com/Search?keyword={keyword}&suggest=1.his.0.0&wq={keyword}&page={page}&s=56&click=0",
                    "search.jd.com");
                //$"/Search?keyword={keyword}&suggest=1.his.0.0&wq={keyword}&page={page}&s=56&click=0");

                // anglesharp
                var searchPageParse = await new HtmlParser(new HtmlParserOptions
                {
                    IsNotConsumingCharacterReferences = true,
                }).ParseDocumentAsync(searchPageHtml);

                var commondities = searchPageParse.QuerySelectorAll(".gl-item"); // 商品，JD的搜索页面一般一页显示三十个
                if (commondities == null || commondities.Count() != 30)
                {
                    _logger.LogError("这个页面不符合JD搜索页面的分析特性，终止爬取作业");
                    continue;
                }

                var skuIds = commondities.Select(u => u.GetAttribute("data-sku")?.Trim());
                skuIds = skuIds.Where(u => u != null).Distinct(); // 去除 none ，再去重
                _logger.LogInformation($"第 {page} 次爬取过程中，在JD搜索页一共获取到 {skuIds.Count()} 个 sku id");

                foreach (var item in skuIds)
                {
                    _logger.LogInformation($"开始顺着SKU为 {item} 的商品进行顺藤摸瓜，抄他家底");
                    jdSkuDtos.Clear();
                    (JdSkuDto JdSkuDto, HashSet<string> adjoinSkuIds) = await GrabDataFromDetailPage(item);

                    if (JdSkuDto is null) // 这一轮失败
                        continue;
                    jdSkuDtos.Add(JdSkuDto);

                    if (adjoinSkuIds == null) // 下一步，存储数据
                    {
                        _logger.LogInformation($"-----------------SKU ID {item} 爬取完毕-----------------");
                        if (await _dataPersistenceService.BatchSaveSkuData(jdSkuDtos))
                            _logger.LogInformation("SKU数据全部保存成功");
                        else
                            _logger.LogError("SKU 数据在保存过程中出现错误导致失败");

                        continue;
                    }

                    foreach (var skuIdItem in adjoinSkuIds) // 循环获取与此 sku 相关的其他sku数据
                    {
                        (JdSkuDto JdSkuDtoTemp, HashSet<string> adjoinSkuIdsTemp) = await GrabDataFromDetailPage(skuIdItem, false);
                        if (JdSkuDtoTemp == null)
                            continue;
                        jdSkuDtos.Add(JdSkuDtoTemp);
                    }

                    // 存储查找出来的数据
                    _logger.LogInformation($"-----------------SKU ID {item} 爬取完毕-----------------");
                    if (await _dataPersistenceService.BatchSaveSkuData(jdSkuDtos))
                        _logger.LogInformation("SKU数据全部保存成功");
                    else
                        _logger.LogError("SKU 数据在保存过程中出现错误导致失败");
                }
            }
        }

        /// <summary>
        /// 抓取详情页数据
        /// </summary>
        /// <param name="skuId">爬取的目标sku</param>
        /// <param name="returnOtherSkuIds">是否返回相邻的sku id集合</param>
        /// <returns></returns>
        protected async Task<(JdSkuDto JdSkuDto, HashSet<string> adjoinSkuIds)> GrabDataFromDetailPage(string skuId, bool returnAdjoinSkuIds = true)
        {
            Thread.Sleep(500);
            JdSkuDto jdSkuDto = new JdSkuDto();
            HashSet<string> adjoinSkuIds = null;

            /* 获取sku详情页的html */
            var html = await SendRequest($"https://item.jd.com/{skuId}.html", "pcitem.jd.hk", $"/{skuId}.html");
            if (html.Length < 500)
            {
                _logger.LogError("这个页面不是正常的商品详情页");
                return (null, null);
            }

            // anglesharp
            var detailPageParse = await new HtmlParser(new HtmlParserOptions
            {
                IsNotConsumingCharacterReferences = true,
            }).ParseDocumentAsync(html);

            // 获取 title
            jdSkuDto.Title = detailPageParse.QuerySelector(".sku-name")?.TextContent.Trim();
            if (string.IsNullOrEmpty(jdSkuDto.Title))
            {
                _logger.LogError("获取 sku 的 title 失败");
                return (null, adjoinSkuIds);
            }

            // 获取轮播图片
            jdSkuDto.RotatePictures = detailPageParse.QuerySelector("div > .spec-items")
                                                    ?.QuerySelectorAll("img")
                                                    ?.Select(u => u.GetAttribute("src")
                                                        ?.Replace("s75x75_jfs", "s450x450_jfs").Replace("/n5/", "/n7/"))
                                                    .ToList();
            if (jdSkuDto.RotatePictures != null && jdSkuDto.RotatePictures.Count() > 0)
            {
                jdSkuDto.RotatePictures = jdSkuDto.RotatePictures.Where(u => u.Length > 4).Distinct().Select(u =>
                {
                    if (u.Substring(0, 4) != "http")
                        return "https:" + u;
                    return u;
                }).ToList(); // 图片里 src 的内容格式是： //img.com/sa/cscacscacsca .....
            }
            else
            {
                _logger.LogError("获取 sku 的轮播图片失败");
                return (null, adjoinSkuIds);
            }

            // 获取价格
            var jsonStr = await SendRequest($"https://item-soa.jd.com/getWareBusiness?callback=jQuery8546809&skuId={skuId}&cat=1316%2C1387%2C1425&area=19_1601_36953_0&shopId=10177691&venderId=10317626&paramJson=%7B%22platform2%22%3A%221%22%2C%22colType%22%3A0%2C%22specialAttrStr%22%3A%22p0pppppppppppppppppp%22%2C%22skuMarkStr%22%3A%2200%22%7D&num=1", "pcitem.jd.hk");
            jsonStr = jsonStr?.Replace("jQuery8546809(", "")?.TrimEnd(')');
            if (string.IsNullOrEmpty(jsonStr))
            {
                _logger.LogError("在获取价格的过程中，接受到的报文内容无效");
                return (null, null);
            }

            jdSkuDto.Price = 0;
            JObject jObj = JObject.Parse(jsonStr);
            var priceJObj = jObj["price"];
            if (priceJObj != null && decimal.TryParse(JObject.Parse(priceJObj.ToString())["p"]?.ToString() ?? "0.00", out decimal decimalParseResult))
                jdSkuDto.Price = decimalParseResult;

            if (jdSkuDto.Price == 0)
            {
                _logger.LogError("获取价格失败");
                return (null, null);
            }

            // 获取详情图集
            /*
            response = await httpClient.GetAsync($"https://wqsitem.jd.com/detail/{skuId}_d{skuId}_normal.html");
            if (response.StatusCode != HttpStatusCode.OK)
            {
                _logger.LogError("发送请求时，获取详情图html、css等相关数据时失败");
                return (null, null);
            }
            var htmlContent = await response.Content.ReadAsStringAsync();
            htmlContent = htmlContent.Replace("\\n", "").Replace("\\", "");

            Regex regex = null;
            bool isImgSrc = false; // 是否是 img src 的方式提取
            if(htmlContent.IndexOf("<style></style>") >= 0) // img 标签的形式
            {
                isImgSrc = true;
                htmlContent = htmlContent.Replace("src=\"//", "src=\"https://"); // 首先先集体加上 https
                regex = new Regex("src=\"https://\\w+\"");
            }
            else // div 背景图片的形式
            {
                htmlContent = htmlContent.Replace("url(//", "url(https://"); // 首先先集体加上 https
                regex = new Regex(@"background-image: url\(https://\w+\)");
            }

            var matches = regex.Matches(htmlContent);
            if(matches != null && matches.Count > 0)
            {
                jdSkuDto.DetailContent = new List<string>();
                if (isImgSrc)
                    jdSkuDto.DetailContent = matches.Select(u => u.Value?.Trim().Replace("src=", "").Replace("\"", "")).ToList();
                else
                    jdSkuDto.DetailContent = matches.Select(u => u.Value?.Trim().Replace("background-image: url(", "").Replace(")", "")).ToList();
            }
            */
            jdSkuDto.DetailContent = new List<string>();


            // 获取属性
            jdSkuDto.SkuAttrs = new List<JdSkuDto.JdSkuAttrDto>();
            int attrIndex = 0;
            IElement attrContainer = null;
            IElement elementTemp = null;
            JdSkuDto.JdSkuAttrDto jdSkuAttrDto = null;
            while (attrIndex++ < 10)
            {
                attrContainer = detailPageParse.QuerySelector($"#choose-attr-{attrIndex}");
                if (attrContainer == null)
                    break;

                jdSkuAttrDto = new JdSkuDto.JdSkuAttrDto();
                // 获取属性类型
                jdSkuAttrDto.Type = attrContainer.GetAttribute("data-type")?.Trim();

                elementTemp = attrContainer.QuerySelector(".selected");
                if (elementTemp == null)
                {
                    _logger.LogError("出现异常，此属性集合，没有被选中元素，严重不符合逻辑");
                    _logger.LogInformation("出现逻辑错误，跳过此次查找，继续找下一属性");
                    continue;
                }

                // 获取属性图片
                jdSkuAttrDto.Image = elementTemp.QuerySelector("img")?.GetAttribute("src")?.Trim();
                if (string.IsNullOrEmpty(jdSkuAttrDto.Image))
                {
                    _logger.LogWarning($"查找SKU的第 {attrIndex} 个属性过程中，发现此属性没有图片");
                    jdSkuAttrDto.Image = "";
                }
                else
                {
                    if (jdSkuAttrDto.Image.Length > 2 && jdSkuAttrDto.Image.Substring(0, 2) == "//")
                        jdSkuAttrDto.Image = "https:" + jdSkuAttrDto.Image;
                }

                // 获取属性文本
                jdSkuAttrDto.Name = elementTemp.QuerySelector("i")?.TextContent?.Trim();
                if (string.IsNullOrEmpty(jdSkuAttrDto.Name))
                {
                    _logger.LogWarning($"查找SKU的第 {attrIndex} 个属性过程中，发现此属性没有文本");
                    jdSkuAttrDto.Name = "";
                }

                jdSkuDto.SkuAttrs.Add(jdSkuAttrDto);

                // 获取相邻的其他 sku id
                if (returnAdjoinSkuIds)
                {
                    if (adjoinSkuIds == null)
                        adjoinSkuIds = new HashSet<string>();

                    var items = attrContainer.QuerySelectorAll("div > .item");
                    if (items == null)
                    {
                        _logger.LogError($"在针对Sku id 为 {skuId} 的详情页面进行抓取数据时，发现其没有属性 item 这个css class");

                        return (null, null);
                    }

                    foreach (var item in items)
                    {
                        var skuIdTemp = item.GetAttribute("data-sku")?.Trim();
                        if (!string.IsNullOrEmpty(skuIdTemp) && skuIdTemp != skuId)
                            adjoinSkuIds.Add(skuIdTemp);
                    }
                }
            }

            Random random = new Random();
            jdSkuDto.Inventory = random.Next(10, 1000);
            return (jdSkuDto, adjoinSkuIds);
        }

        /// <summary>
        /// 发送请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="authority"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        protected async Task<string> SendRequest(string url, string authority, string path = "")
        {
            HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;
            request.Headers.Add("cookie", "unpl=V2_ZzNtbURTQh0gDRZceBpZA2JWEwhKURQWfV1DVy4ZWg03A0FVclRCFnQURldnG10UZgIZXkVcQRZFCEdkeBBVAWMDE1VGZxBFLV0CFSNGF1wjU00zQwBBQHcJFF0uSgwDYgcaDhFTQEJ2XBVQL0oMDDdRFAhyZ0AVRQhHZHsaXQdlAhJYRFJzJXI4dmR4EFkFYAYiXHJWc1chVEFceRFYBioDEVxAVUIVcA5DZHopXw%3d%3d; __jdu=1203301474; shshshfpa=d3f72c4b-a3c3-cdbd-29f6-acc384e257ff-1609259208; shshshfpb=ne6m1eeetMm50L%20zC8C6o3w%3D%3D; areaId=19; ipLoc-djd=19-1601-36953-0; user-key=1ac4e3a0-a7c8-40ad-98c8-e573bae56687; cn=0; mt_xid=V2_52007VwMRU19aU1oaTxxsAmBXEwIKWFVGGRtNXhliCkJXQQgFCkxVHQtQZQAQWloLVV5LeRpdBmcfE1JBWFBLH0gSXAxsBhZiX2hSah1KGV8NZwIXU11oVFMc; __jdv=68990090%7Cdirect%7Ct_1000072676_17008_001%7Cshouq%7C-%7C1610509900802; __jdc=122270672; __jda=122270672.1203301474.1609259192.1610527898.1610629799.32; __jdb=122270672.1.1203301474|32.1610629799; shshshfp=3c001818a1c79933dd7cb52929a53358; shshshsID=f9ab57f99af2bf9cb3b446914def6357_1_1610629800039; 3AB9D23F7A4B3C9B=4GTCCSY7NCBDU4J7L2R22PCFA3TDVAHUYU6VECTNERIND7DLE4E5P25SCQ7X6AN5VIVB2PGWVUZSH6THBHFNGHKWSI");
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/537.36";
            request.Headers.Add("authority", authority);
            request.Headers.Add("method", "GET");
            request.Headers.Add("scheme", "https");
            if (path is not null or "")
                request.Headers.Add("path", path);
            //request.Proxy = new WebProxy("47.52.239.156", 3128);
            var responseContent = await (new StreamReader((await request.GetResponseAsync() as HttpWebResponse).GetResponseStream()).ReadToEndAsync());
            return responseContent;
        }
    }
}
