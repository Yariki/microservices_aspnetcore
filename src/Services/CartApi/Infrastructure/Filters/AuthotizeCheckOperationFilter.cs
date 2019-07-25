using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CartApi.Infrastructure.Filters
{
    public class AuthotizeCheckOperationFilter : IOperationFilter
    {
        
        
        public void Apply(Operation operation, OperationFilterContext context)
        {
            var hasAthorize = context.ApiDescription.ControllerAttributes().OfType<AuthorizeAttribute>().Any() ||
                              context.ApiDescription.ActionAttributes().OfType<AuthorizeAttribute>().Any();
            if (hasAthorize)
            {
                operation.Responses.Add("401",new Response(){Description = "Unauthorized"});
                operation.Responses.Add("403", new Response(){Description = "Forbidden"});
                
                operation.Security = new List<IDictionary<string, IEnumerable<string>>>();
                operation.Security.Add(new Dictionary<string, IEnumerable<string>>()
                {
                    {
                        "oauth2", 
                        new []{"basket"}
                    }
                });
            }
        }
    }
}