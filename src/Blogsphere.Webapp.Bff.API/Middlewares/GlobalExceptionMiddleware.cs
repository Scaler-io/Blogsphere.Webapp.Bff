using System.Net;
using System.Net.Mime;
using Blogsphere.Webapp.Bff.Application.Extensions;
using Blogsphere.Webapp.Bff.Domain.Models.Core;
using Blogsphere.Webapp.Bff.Domain.Models.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;

namespace Blogsphere.Webapp.Bff.API.Middlewares
{
    public class GlobalExceptionMiddleware(
        ILogger logger,
        JsonSerializerSettings jsonSerializerSettings,
        IWebHostEnvironment environment) : IMiddleware
    {
        private readonly ILogger _logger = logger;
        private readonly IWebHostEnvironment _environment = environment;
        private readonly JsonSerializerSettings _jsonSerializerSettings = jsonSerializerSettings;

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleGlobalExceptionAsync(context, ex);
            }
        }

        private async Task HandleGlobalExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = MediaTypeNames.Application.Json;
            if (ex is ValidationException validationException)
            {
                await HandleValidationException(context, validationException, _jsonSerializerSettings);
            }
            else
            {
                await HandleGeneralException(context, ex, _jsonSerializerSettings);
            }
        }

        private async Task HandleGeneralException(HttpContext context, Exception ex, JsonSerializerSettings settings)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var response = _environment.IsDevelopment()
                ? new ApiExceptionResponse(ex.Message, ex.StackTrace)
                : new ApiExceptionResponse(ex.Message);

            var jsonResponse = JsonConvert.SerializeObject(response, settings);
            _logger.Here().Error("{@InternalServerError} - {BeautifiedJson}", ErrorCode.InternalServerError, jsonResponse);
            await context.Response.WriteAsync(jsonResponse);
        }

        private async Task HandleValidationException(HttpContext context, ValidationException validationException, JsonSerializerSettings settings)
        {

            #region obsolte
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            var apiValidationResponse = new ApiValidationResponse
            {
                Errors = []
            };

            var fieldLevelError = validationException.Errors.Select(e => new FieldLevelError { Code = e.ErrorCode, Field = e.PropertyName, Message = e.ErrorMessage }).ToList();
            apiValidationResponse.Errors.AddRange(fieldLevelError);

            _logger.Here().Error("{@BadRequest} - {@response}", ErrorCode.BadRequest, apiValidationResponse);
            await context.Response.WriteAsync(JsonConvert.SerializeObject(apiValidationResponse, settings));
            #endregion

            // Ensure ModelState is initialized in HttpContext.Items
            context.Items["ModelState"] ??= new ModelStateDictionary();
            var modelState = (ModelStateDictionary)context.Items["ModelState"];

            // Add FluentValidation errors to ModelState
            foreach (var error in validationException.Errors)
            {
                modelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }

            // Trigger the InvalidModelStateResponseFactory by setting ModelState and ending the pipeline
            await context.Response.CompleteAsync();  // Stops the request pipeline here
        }
    }
}