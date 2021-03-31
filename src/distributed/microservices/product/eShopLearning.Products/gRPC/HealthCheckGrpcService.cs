using Grpc.Core;
using Grpc.Health.V1;
using Grpc.HealthCheck;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.Products.gRPC
{
    public class HealthCheckGrpcService : HealthServiceImpl
    {
        public override Task<HealthCheckResponse> Check(HealthCheckRequest request, ServerCallContext context)
        {
            //TODO:检查逻辑
            return Task.FromResult(new HealthCheckResponse() { Status = HealthCheckResponse.Types.ServingStatus.Serving });
        }

        public override async Task Watch(HealthCheckRequest request, IServerStreamWriter<HealthCheckResponse> responseStream, ServerCallContext context)
        {
            //TODO:检查逻辑
            await responseStream.WriteAsync(new HealthCheckResponse()
            { Status = HealthCheckResponse.Types.ServingStatus.Serving });
        }
    }
}
