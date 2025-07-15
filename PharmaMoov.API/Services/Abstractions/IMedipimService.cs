

using Microsoft.Extensions.Configuration;
using PharmaMoov.Models.External.Medipim;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PharmaMoov.API.Services.Abstractions
{
    public interface IMedipimService
    {
        Task<List<MedipimCategoryDto>> GetPublicCategoriesAsync();
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
            var result = await JsonSerializer.DeserializeAsync<MedipimCategoryResponse>(stream, options);

            return result?.Results ?? new List<MedipimCategoryDto>();
        }
    }

}