using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace EqDemo.Controllers
{
    [Route("eqproxy")]
    public class EqProxyController : Controller
    {

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _serviceBaseUrl;

        public EqProxyController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _serviceBaseUrl = configuration["ServiceApp:BaseUrl"] ?? "https://localhost:44331/api/easyquery/";
        }

        private HttpClient CreateClient()
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_serviceBaseUrl);
            return client;
        }

        [HttpGet]
        [Route("lck")]
        public async Task<IActionResult> GetLicenseKeyAsync(CancellationToken ct)
        {
            using var client = CreateClient();
            var response = await client.GetAsync("lck", ct);
            var key = await response.Content.ReadAsStringAsync(ct);
            return Content(key.Substring(1, key.Length - 2));
        }

        [HttpGet]
        [Route("models/{modelId}")]
        public async Task<IActionResult> GetModelAsync(string modelId, CancellationToken ct)
        {
            using var client = CreateClient();
            client.DefaultRequestHeaders.TryAddWithoutValidation("x-eqjs-version", "7.1.0");
            var response = await client.GetAsync($"models/{modelId}", ct);
            var content = await response.Content.ReadAsStringAsync(ct);
            return JsonContent(content, response.StatusCode);
        }

        [HttpGet]
        [Route("models/{modelId}/queries/{queryId}")]
        public async Task<IActionResult> GetQueryAsync(string modelId, string queryId, CancellationToken ct)
        {
            using var client = CreateClient();
            var response = await client.GetAsync($"models/{modelId}/queries/{queryId}", ct);
            var content = await response.Content.ReadAsStringAsync(ct);
            return JsonContent(content, response.StatusCode);
        }

        [HttpGet]
        [Route("models/{modelId}/queries")]
        public async Task<IActionResult> GetQueryListAsync(string modelId, CancellationToken ct)
        {
            using var client = CreateClient();
            var response = await client.GetAsync($"models/{modelId}/queries", ct);
            var content = await response.Content.ReadAsStringAsync(ct);
            return JsonContent(content, response.StatusCode);
        }

        [HttpPost]
        [Route("models/{modelId}/queries")]
        public async Task<IActionResult> NewQueryAsync(string modelId, CancellationToken ct)
        {
            using var client = CreateClient();
            using var reader = new StreamReader(Request.Body);
            var bodyJson = await reader.ReadToEndAsync(ct);

            var requestContent = new StringContent(bodyJson);
            var response = await client.PostAsync($"models/{modelId}/queries", requestContent, ct);
            var content = await response.Content.ReadAsStringAsync(ct);
            return JsonContent(content, response.StatusCode);
        }

        [HttpPut]
        [Route("models/{modelId}/queries/{queryId}")]
        public async Task<IActionResult> SaveQueryAsync(string modelId, string queryId, CancellationToken ct)
        {
            using var client = CreateClient();
            using var reader = new StreamReader(Request.Body);
            var bodyJson = await reader.ReadToEndAsync(ct);

            var requestContent = new StringContent(bodyJson);
            var response = await client.PutAsync($"models/{modelId}/queries/{queryId}", requestContent, ct);
            var content = await response.Content.ReadAsStringAsync(ct);
            return JsonContent(content, response.StatusCode);
        }

        [HttpDelete]
        [Route("models/{modelId}/queries/{queryId}")]
        public async Task<IActionResult> RemoveQueryAsync(string modelId, string queryId, CancellationToken ct)
        {
            using var client = CreateClient();
            var response = await client.DeleteAsync($"models/{modelId}/queries/{queryId}", ct);
            var content = await response.Content.ReadAsStringAsync(ct);
            return JsonContent(content, response.StatusCode);
        }

        [HttpPost]
        [Route("models/{modelId}/queries/{queryId}/sync")]
        public async Task<IActionResult> SyncQueryAsync(string modelId, string queryId, CancellationToken ct)
        {
            using var client = CreateClient();
            using var reader = new StreamReader(Request.Body);
            var bodyJson = await reader.ReadToEndAsync(ct);

            var requestContent = new StringContent(bodyJson);
            var response = await client.PostAsync($"models/{modelId}/queries/{queryId}/sync", requestContent, ct);
            var content = await response.Content.ReadAsStringAsync(ct);
            return JsonContent(content, response.StatusCode);
        }

        [HttpPost]
        [Route("models/{modelId}/fetch")]
        public async Task<IActionResult> FetchDataAsync(string modelId, CancellationToken ct)
        {
            using var client = CreateClient();
            using var reader = new StreamReader(Request.Body);
            var bodyJson = await reader.ReadToEndAsync(ct);

            var requestContent = new StringContent(bodyJson);
            var response = await client.PostAsync($"models/{modelId}/fetch", requestContent, ct);
            var content = await response.Content.ReadAsStringAsync(ct);
            return JsonContent(content, response.StatusCode);
        }

        [HttpGet]
        [Route("models/{modelId}/valuelists/{editorId}")]
        public async Task<IActionResult> GetValueListAsync(string modelId, string editorId, CancellationToken ct)
        {
            using var client = CreateClient();
            var response = await client.GetAsync($"models/{modelId}/valuelists/{editorId}", ct);
            var content = await response.Content.ReadAsStringAsync(ct);
            return JsonContent(content, response.StatusCode);
        }

        [HttpPost]
        [Route("models/{modelId}/export/{formatType}")]
        public async Task<IActionResult> ExportResultAsync(string modelId, string formatType, CancellationToken ct)
        {
            using var client = CreateClient();
            using var reader = new StreamReader(Request.Body);
            var bodyJson = await reader.ReadToEndAsync(ct);

            var requestContent = new StringContent(bodyJson);
            var response = await client.PostAsync($"models/{modelId}/export/{formatType}", requestContent, ct);
            if (response.IsSuccessStatusCode) {
                var content = await response.Content.ReadAsByteArrayAsync(ct);
                var contentType = response.Content.Headers.ContentType.MediaType;
                var fileName = response.Content.Headers.ContentDisposition.FileName;
                return File(content, contentType, fileName.Substring(1, fileName.Length - 2));
            }
            else {
                var content = await response.Content.ReadAsStringAsync(ct);
                return JsonContent(content, response.StatusCode);
            }
        }

        private static readonly UTF8Encoding Utf8NoBomEncoding = new UTF8Encoding(false);
        private ContentResult JsonContent(string json, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            HttpContext.Response.StatusCode = (int)statusCode;
            return Content(json, "application/json", Utf8NoBomEncoding);
        }
    }
}
