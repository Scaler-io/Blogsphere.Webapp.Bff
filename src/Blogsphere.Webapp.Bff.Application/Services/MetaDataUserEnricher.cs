using AutoMapper;
using Blogsphere.Webapp.Bff.Application.Contracts.ApiProvider;
using Blogsphere.Webapp.Bff.Application.Contracts.Enrichment;
using Blogsphere.Webapp.Bff.Application.Extensions;
using Blogsphere.Webapp.Bff.Domain.Models.Constants;
using Blogsphere.Webapp.Bff.Domain.Models.Core;
using Blogsphere.Webapp.Bff.Domain.Models.Dtos;
using Blogsphere.Webapp.Bff.Domain.Models.Enums;

namespace Blogsphere.Webapp.Bff.Application.Services
{
    public sealed class MetaDataUserEnricher(
        ILogger logger,
        IUserApiProvider userApiProvider,
        IMapper mapper) : IMetaDataUserEnricher
    {
        private readonly ILogger _logger = logger;
        private readonly IUserApiProvider _userApiProvider = userApiProvider;
        private readonly IMapper _mapper = mapper;

        public async Task<Result<MetaDataDto>> EnrichAsync(
            Domain.Entities.MetaData sourceMetaData,
            MetaDataDto targetMetaData,
            RequestInformation requestInformation,
            CancellationToken cancellationToken = default)
        {
            if (targetMetaData is null)
            {
                return Result<MetaDataDto>.Failure(ErrorCode.InternalServerError, ErrorMessages.InternalServerError);
            }

            var createdById = sourceMetaData?.CreatedBy;
            var updatedById = sourceMetaData?.UpdatedBy;

            var userIds = new[] { createdById, updatedById }
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            if (userIds.Length == 0)
            {
                return Result<MetaDataDto>.Success(targetMetaData);
            }

            var userTaskById = userIds.ToDictionary(
                id => id,
                id => _userApiProvider.GetManagementUserNameDetailsById(id, requestInformation, cancellationToken),
                StringComparer.Ordinal);

            await Task.WhenAll(userTaskById.Values);

            foreach (var (userId, userTask) in userTaskById)
            {
                var userResult = await userTask;
                if (userResult is null)
                {
                    _logger.Here()
                        .WithCorrelationId(requestInformation.CorreationId)
                        .Error("User lookup returned null for id: {Id}", userId);

                    return Result<MetaDataDto>.Failure(ErrorCode.InternalServerError, ErrorMessages.InternalServerError);
                }

                if (!userResult.IsSuccess)
                {
                    _logger.Here()
                        .WithCorrelationId(requestInformation.CorreationId)
                        .Error(
                            "User lookup failed for id: {Id}. ErrorCode: {ErrorCode}. ErrorMessage: {ErrorMessage}",
                            userId,
                            userResult.ErrorCode,
                            userResult.ErrorMessage);

                    return Result<MetaDataDto>.Failure(userResult.ErrorCode, userResult.ErrorMessage);
                }

                if (userResult.Data is null)
                {
                    _logger.Here()
                        .WithCorrelationId(requestInformation.CorreationId)
                        .Error("User lookup returned success but Data is null for id: {Id}", userId);

                    return Result<MetaDataDto>.Failure(ErrorCode.InternalServerError, ErrorMessages.InternalServerError);
                }

                if (string.Equals(userId, createdById, StringComparison.Ordinal))
                {
                    targetMetaData.CreatedBy = _mapper.Map<OperationaUserDetailsDto>(userResult.Data);
                }

                if (string.Equals(userId, updatedById, StringComparison.Ordinal))
                {
                    targetMetaData.UpdatedBy = _mapper.Map<OperationaUserDetailsDto>(userResult.Data);
                }
            }

            return Result<MetaDataDto>.Success(targetMetaData);
        }
    }

}
