using Microsoft.Extensions.Configuration;
using PharmaMoov.Models.External.Medipim;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PharmaMoov.API.Services.Abstractions
{
    public interface IMedipimService
    {
        Task<List<MedipimCategoryDto>> GetPublicCategoriesAsync();
        Task<List<MedipimProductDto>> GetProductsAsync(GetMedipimProductsRequest request);
    }

    public class MedipimService : IMedipimService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public MedipimService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<List<MedipimCategoryDto>> GetPublicCategoriesAsync()
        {
            var baseUrl = _configuration["Medipim:BaseUrl"];
            var apiId = _configuration["Medipim:ApiId"];
            var apiKey = _configuration["Medipim:ApiKey"];

            var credentials = $"{apiId}:{apiKey}";
            var base64Creds = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));

            using var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v4/public-categories/query");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Creds);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent("{}", Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = await System.Text.Json.JsonSerializer.DeserializeAsync<MedipimCategoryResponse>(stream, options);

            return result?.Results ?? new List<MedipimCategoryDto>();
        }

        public async Task<List<MedipimProductDto>> GetProductsAsync(GetMedipimProductsRequest request)
        {
            var baseUrl = _configuration["Medipim:BaseUrl"];
            var apiId = _configuration["Medipim:ApiId"];
            var apiKey = _configuration["Medipim:ApiKey"];

            var credentials = $"{apiId}:{apiKey}";
            var base64Creds = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));

            var url = $"{baseUrl}/v4/products/query";

            var body = new
            {
                filter = new
                {
                    and = new object[]
                    {
                        new { status = "active" },
                        new { minimumContent = true },
                        new { updatedSince = request.UpdatedSince }
                    }
                },
                sorting = new
                {
                    name = new { direction = "ASC", locale = "en" }
                },
                page = new
                {
                    no = request.Page,
                    size = request.PageSize
                }
            };

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Creds);
            httpRequest.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<MedipimProductsApiResponse>(json);

            return result?.Results ?? new List<MedipimProductDto>();
        }

        private class MedipimProductsApiResponse
        {
            [JsonProperty("results")]
            public List<MedipimProductDto> Results { get; set; }
        }

        private class MedipimCategoryResponse
        {
            [JsonProperty("results")]
            public List<MedipimCategoryDto> Results { get; set; }
        }
    }
}
