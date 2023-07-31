using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace week2_homework
{
    public class OcaTrigger
    {
        private readonly ILogger _logger;

        public OcaTrigger(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<OcaTrigger>();
        }

        [Function("Greetings")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" }, Summary = "The name of the person to use in the greeting.", Description = "This HTTP triggered function returns a person's name.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Summary = "The name of the person to use in the greeting.", Description = "The name of the person to use in the greeting.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: System.Net.HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Summary = "The response", Description = "This returns the response")]
        [OpenApiResponseWithBody(statusCode: System.Net.HttpStatusCode.InternalServerError, contentType: "text/plain", bodyType: typeof(string), Summary = "The response", Description = "This returns the response")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString("Welcome to Azure Functions!");

            return response;
        }
    }
}
