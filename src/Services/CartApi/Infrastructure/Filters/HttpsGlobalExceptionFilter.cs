using System.Net;
using CartApi.Infrastructure.ActionResults;
using CartApi.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace CartApi.Infrastructure.Filters
{
    public class HttpsGlobalExceptionFilter : IExceptionFilter
    {
        private readonly IHostingEnvironment _environment;
        private readonly ILogger<HttpsGlobalExceptionFilter> _logger;

        public HttpsGlobalExceptionFilter(IHostingEnvironment environment, ILogger<HttpsGlobalExceptionFilter> logger)
        {
            _environment = environment;
            _logger = logger;
        }


        public void OnException(ExceptionContext context)
        {
            _logger.LogError(new EventId(context.Exception.HResult),
                context.Exception, context.Exception.Message);

            if (context.Exception is CartDomainException cartDomainException)
            {

                var json = new JsonErrorResponse
                {
                    Messages = new[] {context.Exception.Message}
                };
                context.Result = new BadRequestObjectResult(json);
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
            else
            {
                var json = new JsonErrorResponse()
                {
                    Messages = new []{"An error occured. Try it again"}
                };
                if (_environment.IsDevelopment())
                {
                    json.DeveloperMessage = context.Exception;
                }
                context.Result = new InternetServerErrorObjectResult(json);
                context.HttpContext.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
            }

            context.ExceptionHandled = true;
        }
    }

    class JsonErrorResponse
    {
        public string[] Messages { get; set; }


        public object DeveloperMessage { get; set; }
    }
}