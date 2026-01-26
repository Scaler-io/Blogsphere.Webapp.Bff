using AutoMapper;
using Blogsphere.Webapp.Bff.Application.Contracts.ApiProvider;
using Blogsphere.Webapp.Bff.Application.Contracts.CQRS;
using Blogsphere.Webapp.Bff.Application.Extensions;
using Blogsphere.Webapp.Bff.Domain.Entities.ManagementUsers;
using Blogsphere.Webapp.Bff.Domain.Models.Constants;
using Blogsphere.Webapp.Bff.Domain.Models.Core;
using Blogsphere.Webapp.Bff.Domain.Models.Dtos;
using Blogsphere.Webapp.Bff.Domain.Models.Enums;

namespace Blogsphere.Webapp.Bff.Application.Features.ApiClusters.Queries.GetApiClusterById
{
    public class GetApiClusterByIdQueryHandler(
        ILogger logger,
        IApiGatewayProvider apiGatewayProvider,
        IUserApiProvider userApiProvider,
        IMapper mapper) : IQueryHandler<GetApiClusterByIdQuery, Result<ApiClusterDto>>
    {
        private readonly ILogger _logger = logger;
        private readonly IApiGatewayProvider _apiGatewayProvider = apiGatewayProvider;
        private readonly IUserApiProvider _userApiProvider = userApiProvider;
        private readonly IMapper _mapper = mapper;

        public async Task<Result<ApiClusterDto>> Handle(GetApiClusterByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.Here().MethodEntered();
            _logger.Here().WithCorrelationId(request.RequestInformation.CorreationId).Information("Getting api cluster by id: {Id}", request.Id);

            var apiCluster = await _apiGatewayProvider.GetApiCLusterDetailsByIdAsync(request.Id, request.RequestInformation, cancellationToken);

            if (!apiCluster.IsSuccess)
            {
                _logger.Here().WithCorrelationId(request.RequestInformation.CorreationId).Error("Failed to get api cluster by id: {Id}", request.Id);
                return Result<ApiClusterDto>.Failure(apiCluster.ErrorCode, apiCluster.ErrorMessage);
            }

            var apiClusterDto = _mapper.Map<ApiClusterDto>(apiCluster.Data);
            apiClusterDto.MetaData ??= new MetaDataDto();

            var createdBy = apiCluster.Data.MetaData.CreatedBy;
            var updatedBy = apiCluster.Data.MetaData.UpdatedBy;

            var userIds = new[] { createdBy, updatedBy }
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            if (userIds.Length > 0)
            {

                var userTaskById = userIds.ToDictionary(
                    id => id,
                    id => _userApiProvider.GetManagementUserNameDetailsById(id, request.RequestInformation, cancellationToken),
                    StringComparer.Ordinal);

                await Task.WhenAll(userTaskById.Values);

                foreach (var (userId, userTask) in userTaskById)
                {
                    var userResult = await userTask;
                    if (userResult is null)
                    {
                        _logger.Here().WithCorrelationId(request.RequestInformation.CorreationId).Error("User lookup returned null for id: {Id}", userId);
                        return Result<ApiClusterDto>.Failure(ErrorCode.InternalServerError, ErrorMessages.InternalServerError);
                    }
                    if (!userResult.IsSuccess)
                    {
                        _logger.Here().WithCorrelationId(request.RequestInformation.CorreationId).Error(
                            "User lookup failed for id: {Id}. ErrorCode: {ErrorCode}. ErrorMessage: {ErrorMessage}",
                            userId,
                            userResult.ErrorCode,
                            userResult.ErrorMessage);
                        return Result<ApiClusterDto>.Failure(userResult.ErrorCode, userResult.ErrorMessage);
                    }

                    if (string.Equals(userId, createdBy, StringComparison.Ordinal))
                    {
                        apiClusterDto.MetaData.CreatedBy = ToOperationalUserDetailsDto(userResult.Data);
                    }

                    if (string.Equals(userId, updatedBy, StringComparison.Ordinal))
                    {
                        apiClusterDto.MetaData.UpdatedBy = ToOperationalUserDetailsDto(userResult.Data);
                    }
                }
            }

            return Result<ApiClusterDto>.Success(apiClusterDto);
        }

        private static OperationaUserDetailsDto ToOperationalUserDetailsDto(ManagementUserDetails user)
        {
            return new()
            {
                FullName = user.FullName,
                Email = user.Email,
                EmployeeId = user.EmployeeId,
                JobTitle = user.JobTitle,
            };
        }
    }
}
