using System.Text;
using Blogsphere.Webapp.Bff.Application.Contracts.ApiProvider;
using Blogsphere.Webapp.Bff.Application.Contracts.Factory;
using Blogsphere.Webapp.Bff.Application.Contracts.TokenExchange;
using Blogsphere.Webapp.Bff.Domain.Entities.Search;
using Blogsphere.Webapp.Bff.Domain.Models.Constants;
using Blogsphere.Webapp.Bff.Domain.Models.Core;
using Blogsphere.Webapp.Bff.Domain.Models.Enums;
using Newtonsoft.Json;

namespace Blogsphere.Webapp.Bff.Infrastructure.Providers
{
    public class SearchApiProvider(
        ILogger logger,
        IHttpClientFactory httpClientFactory,
        ITokenExchangeService tokenExchangeService,
        ICacheServiceFactory cacheServiceFactory,
        JsonSerializerSettings jsonSerializerSettings,
        string apiProviderName) : BaseProvider(logger, httpClientFactory, tokenExchangeService, cacheServiceFactory.GetCacheService(CacheServiceType.Distributed), apiProviderName), ISearchApiProvider
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings = jsonSerializerSettings;

        public async Task<Result<PaginatedResult<ApiClusterSummary>>> SearchClustersAsync(
            PaginatedSearchRequest request,
            RequestInformation requestInformation,
            CancellationToken cancellationToken = default)
        {
            var searchApiHttpClient = await GetHttpClientAsync(requestInformation, isPublic: true);

            if (searchApiHttpClient is null)
            {
                return Result<PaginatedResult<ApiClusterSummary>>.Failure(ErrorCode.InternalServerError, ErrorMessages.InternalServerError);
            }

            SetSearchApiHeaders(searchApiHttpClient);

            var requestBody = new StringContent(JsonConvert.SerializeObject(request, _jsonSerializerSettings), Encoding.UTF8, "application/json");
            
            var response = await searchApiHttpClient.PostAsync($"apicluster-search-index", requestBody, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return Result<PaginatedResult<ApiClusterSummary>>.Failure(ErrorCode.InternalServerError, ErrorMessages.InternalServerError);
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var clusters = JsonConvert.DeserializeObject<PaginatedResult<ApiClusterSummary>>(content, _jsonSerializerSettings);
            return Result<PaginatedResult<ApiClusterSummary>>.Success(clusters);
        }
        public async Task<Result<PaginatedResult<ApiRouteSummary>>> SearchRoutesAsync(
            PaginatedSearchRequest request,
            RequestInformation requestInformation,
            CancellationToken cancellationToken = default)
        {
            var searchApiHttpClient = await GetHttpClientAsync(requestInformation, isPublic: true);
            if (searchApiHttpClient is null)
            {
                return Result<PaginatedResult<ApiRouteSummary>>.Failure(ErrorCode.InternalServerError, ErrorMessages.InternalServerError);
            }

            SetSearchApiHeaders(searchApiHttpClient);

            var requestBody = new StringContent(JsonConvert.SerializeObject(request, _jsonSerializerSettings), Encoding.UTF8, "application/json");
            var response = await searchApiHttpClient.PostAsync($"apiroute-search-index", requestBody, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return Result<PaginatedResult<ApiRouteSummary>>.Failure(ErrorCode.InternalServerError, ErrorMessages.InternalServerError);
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var routes = JsonConvert.DeserializeObject<PaginatedResult<ApiRouteSummary>>(content, _jsonSerializerSettings);
            return Result<PaginatedResult<ApiRouteSummary>>.Success(routes);
        }

        public async Task<Result<PaginatedResult<ManagementUserSummary>>> SearchManagementUsersAsync(
            PaginatedSearchRequest request,
            RequestInformation requestInformation,
            CancellationToken cancellationToken = default)
        {
            var searchApiHttpClient = await GetHttpClientAsync(requestInformation, isPublic: true);
            if (searchApiHttpClient is null)
            {
                return Result<PaginatedResult<ManagementUserSummary>>.Failure(ErrorCode.InternalServerError, ErrorMessages.InternalServerError);
            }

            SetSearchApiHeaders(searchApiHttpClient);

            var requestBody = new StringContent(JsonConvert.SerializeObject(request, _jsonSerializerSettings), Encoding.UTF8, "application/json");
            var response = await searchApiHttpClient.PostAsync($"managementuser-search-index", requestBody, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return Result<PaginatedResult<ManagementUserSummary>>.Failure(ErrorCode.InternalServerError, ErrorMessages.InternalServerError);
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var managementUsers = JsonConvert.DeserializeObject<PaginatedResult<ManagementUserSummary>>(content, _jsonSerializerSettings);
            return Result<PaginatedResult<ManagementUserSummary>>.Success(managementUsers);
        }

        public async Task<Result<long>> GetTotalClustersCountAsync(
            RequestInformation requestInformation,
            CancellationToken cancellationToken = default)
        {
            var searchApiHttpClient = await GetHttpClientAsync(requestInformation, isPublic: true);
            if (searchApiHttpClient is null)
            {
                return Result<long>.Failure(ErrorCode.InternalServerError, ErrorMessages.InternalServerError);
            }

            SetSearchApiHeaders(searchApiHttpClient);

            var response = await searchApiHttpClient.PostAsync($"count/apicluster-search-index", null, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return Result<long>.Failure(ErrorCode.InternalServerError, ErrorMessages.InternalServerError);
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var count = JsonConvert.DeserializeObject<long>(content, _jsonSerializerSettings);
            return Result<long>.Success(count);
        }

        public async Task<Result<long>> GetTotalRoutesCountAsync(
            RequestInformation requestInformation,
            CancellationToken cancellationToken = default)
        {
            var searchApiHttpClient = await GetHttpClientAsync(requestInformation, isPublic: true);
            if (searchApiHttpClient is null)
            {
                return Result<long>.Failure(ErrorCode.InternalServerError, ErrorMessages.InternalServerError);
            }

            SetSearchApiHeaders(searchApiHttpClient);

            var response = await searchApiHttpClient.PostAsync($"count/apiroute-search-index", null, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return Result<long>.Failure(ErrorCode.InternalServerError, ErrorMessages.InternalServerError);
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var count = JsonConvert.DeserializeObject<long>(content, _jsonSerializerSettings);
            return Result<long>.Success(count);
        }

        public async Task<Result<long>> GetTotalManagementUsersCountAsync(
            RequestInformation requestInformation,
            CancellationToken cancellationToken = default)
        {
            var searchApiHttpClient = await GetHttpClientAsync(requestInformation, isPublic: true);
            if (searchApiHttpClient is null)
            {
                return Result<long>.Failure(ErrorCode.InternalServerError, ErrorMessages.InternalServerError);
            }

            SetSearchApiHeaders(searchApiHttpClient);

            var response = await searchApiHttpClient.PostAsync($"count/managementuser-search-index", null, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return Result<long>.Failure(ErrorCode.InternalServerError, ErrorMessages.InternalServerError);
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var count = JsonConvert.DeserializeObject<long>(content, _jsonSerializerSettings);
            return Result<long>.Success(count);
        }

        private static void SetSearchApiHeaders(HttpClient httpClient)
        {
            httpClient.DefaultRequestHeaders.Add("api-version", "v1");
        }
    }
}
