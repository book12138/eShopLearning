using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace eShopLearning.Common.HttpServices.HttpMessageHandler
{
    public class DevspacesMessageHandler : DelegatingHandler
    {
        private const string DevspacesHeaderName = "azds-route-as";
        private readonly IHttpContextAccessor _httpContextAccessor;
        public DevspacesMessageHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var req = _httpContextAccessor.HttpContext.Request;

            if (req.Headers.ContainsKey(DevspacesHeaderName))
            {
                request.Headers.Add(DevspacesHeaderName, req.Headers[DevspacesHeaderName] as IEnumerable<string>);
            }
            return base.SendAsync(request, cancellationToken);
        }
    }
}
