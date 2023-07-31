using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks; // 비동기 작업을 위해 필요

namespace week3_homework
{
    public class ChatGPT
    {
        private readonly ILogger<ChatGPT> _logger; // 제네릭 타입 지정
        private readonly IConfiguration _configuration;

        public ChatGPT(ILogger<ChatGPT> logger, IConfiguration configuration) // 생성자 수정
        {
            _logger = logger;
            _configuration = configuration;
        }

        [Function("ChatGPT")]
        [OpenApiOperation(operationId: nameof(ChatGPT.Run), tags: new[] { "name" })]
        [OpenApiRequestBody(contentType: "text/plain", bodyType: typeof(string), Required = true, Description = "The request body")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "completions")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            string apiKey = _configuration["GPT_KEY"] ?? string.Empty; // null 처리
            string endpoint = _configuration["GPT_URL"] ?? string.Empty; // null 처리
            
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var question = req.ReadAsString() ?? string.Empty;

            var requestData = JsonSerializer.Serialize(new
            {
                // 4는 가격이 비싸니 3.5로 설정한다.
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    // role은 system, assistant, user 가능하다.
                    new { role = "system", content = "너는 내가 질문하는 것에 2~3줄로 답변해주면 고맙겠어, 답변할 때는 요약해서 앞에 - 를 붙여서 대답해줘 답변은 한국어로 해줘" },
                    new { role = "user", content = question ?? string.Empty }
                },
                temperature = 0.7f,
            });
            var content = new StringContent(requestData, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(endpoint, content);
            if(response.StatusCode == HttpStatusCode.OK){
                // ResponseBody만들기(OK일 경우에만)
                var responseBody = await response.Content.ReadAsStringAsync();
                JObject jObject = JObject.Parse(responseBody);
                string result = string.Empty;
                var choices = jObject["choices"];
                if (choices != null && choices.HasValues)
                {
                    var messageContent = choices[0]?["message"]?["content"];
                    if (messageContent != null)
                    {
                        result = (string)messageContent;
                    }
                }
                // 선택자로 선택 후 해당 내용 String으로 변환, 추가
                var httpResponse = req.CreateResponse(HttpStatusCode.OK);
                httpResponse.Headers.Add("Content-Type", "text/plain; charset=utf-8");
                if (result != null)
                    httpResponse.WriteString(result);
                return httpResponse;
            }
            else{
                var httpResponse = req.CreateResponse(response.StatusCode);
                httpResponse.Headers.Add("Content-Type", "text/plain; charset=utf-8");
                httpResponse.WriteString("Error in calling GPT");
                return httpResponse;
            }
        }
    }
}
