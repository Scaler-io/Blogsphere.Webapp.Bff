using Blogsphere.Webapp.Bff.Application.Contracts.ApiProvider;
using Blogsphere.Webapp.Bff.Application.Contracts.Factory;
using Blogsphere.Webapp.Bff.Application.Contracts.TokenExchange;
using Blogsphere.Webapp.Bff.Domain.Entities.ApiGateway;
using Blogsphere.Webapp.Bff.Domain.Models.Constants;
using Blogsphere.Webapp.Bff.Domain.Models.Core;
using Blogsphere.Webapp.Bff.Domain.Models.Dtos;
using Blogsphere.Webapp.Bff.Domain.Models.Enums;
using Newtonsoft.Json;

namespace Blogsphere.Webapp.Bff.Infrastructure.Providers
{
    public class ApiGatewayProvider(
        ILogger logger,
        IHttpClientFactory httpClientFactory,
        ITokenExchangeService tokenExchangeService,
        ICacheServiceFactory cacheServiceFactory,
        string apiProviderName,
        JsonSerializerSettings jsonSerializerSettings) : BaseProvider(logger, httpClientFactory, tokenExchangeService, cacheServiceFactory.GetCacheService(CacheServiceType.Distributed), apiProviderName)
        , IApiGatewayProvider
    {

        private readonly JsonSerializerSettings _jsonSerializerSettings = jsonSerializerSettings;
        public async Task<Result<ApiCluster>> GetApiCLusterDetailsByIdAsync(string id, RequestInformation requestInformation, CancellationToken cancellationToken = default)
        {
            var apiGatewayHttpClient = await GetHttpClientAsync(requestInformation, "apigateway:read");
            if (apiGatewayHttpClient.DefaultRequestHeaders.Contains("CorrelationId"))
            {
                apiGatewayHttpClient.DefaultRequestHeaders.Remove("CorrelationId");
            }

            apiGatewayHttpClient.DefaultRequestHeaders.Add("CorrelationId", requestInformation.CorreationId);

            var response = await apiGatewayHttpClient.GetAsync($"/api/v1/proxycluster/{id}", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return Result<ApiCluster>.Failure(ErrorCode.OperationFailed, ErrorMessages.Operationfailed);
            }
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var apiCluster = JsonConvert.DeserializeObject<ApiCluster>(content, _jsonSerializerSettings);
            return Result<ApiCluster>.Success(apiCluster);
        }

        public async Task<Result<ApiRoute>> GetApiRouteByIdAsync(string id, RequestInformation requestInformation, CancellationToken cancellationToken = default)
        {
            var apiGatewayHttpClient = await GetHttpClientAsync(requestInformation, "apigateway:read");
            if (apiGatewayHttpClient.DefaultRequestHeaders.Contains("CorrelationId"))
            {
                apiGatewayHttpClient.DefaultRequestHeaders.Remove("CorrelationId");
            }

            apiGatewayHttpClient.DefaultRequestHeaders.Add("CorrelationId", requestInformation.CorreationId);

            var response = await apiGatewayHttpClient.GetAsync($"/api/v1/proxyroute/{id}", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return Result<ApiRoute>.Failure(ErrorCode.OperationFailed, ErrorMessages.Operationfailed);
            }
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var apiRoute = JsonConvert.DeserializeObject<ApiRoute>(content, _jsonSerializerSettings);
            return Result<ApiRoute>.Success(apiRoute);
        }

        public async Task<Result<ApiProductListDto>> GetApiProductsAsync(RequestInformation requestInformation, CancellationToken cancellationToken = default)
        {
            var apiGatewayHttpClient = await GetHttpClientAsync(requestInformation, scope: "apigateway:read");
            if (apiGatewayHttpClient.DefaultRequestHeaders.Contains("CorrelationId"))
            {
                apiGatewayHttpClient.DefaultRequestHeaders.Remove("CorrelationId");
            }

            apiGatewayHttpClient.DefaultRequestHeaders.Add("CorrelationId", requestInformation.CorreationId);

            var response = await apiGatewayHttpClient.GetAsync($"/api/v1/apiproduct?page=1&pageSize=1", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return Result<ApiProductListDto>.Failure(ErrorCode.OperationFailed, ErrorMessages.Operationfailed);
            }
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var apiProducts = JsonConvert.DeserializeObject<PaginatedResultV2<ApiProduct>>(content, _jsonSerializerSettings);

            var allApiProducts = new List<ApiProduct>();
            while (apiProducts.HasNextPage)
            {
                response = await apiGatewayHttpClient.GetAsync($"/api/v1/apiproduct?page={apiProducts.PageNumber + 1}", cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    return Result<ApiProductListDto>.Failure(ErrorCode.OperationFailed, ErrorMessages.Operationfailed);
                }
                content = await response.Content.ReadAsStringAsync(cancellationToken);
                apiProducts = JsonConvert.DeserializeObject<PaginatedResultV2<ApiProduct>>(content, _jsonSerializerSettings);
                allApiProducts.AddRange(apiProducts.Items);
            }
            return Result<ApiProductListDto>.Success(new ApiProductListDto
            {
                ApiProducts = allApiProducts.AsReadOnly(),
                TotalCount = apiProducts.TotalCount
            });
        }
    }
}