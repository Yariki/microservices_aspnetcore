using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http2.HPack;

namespace CartApi.Infrastructure.ActionResults
{
    public class InternetServerErrorObjectResult : ObjectResult
    {
        public InternetServerErrorObjectResult(object value) : base(value)
        {
            StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError;
        }
    }
}