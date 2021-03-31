using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace eShopLearning.HttpAggregator.ApplicationServices.Impl
{
    public class CartService : ICartService
    {
        private readonly HttpClient _httpClient;
    }
}
