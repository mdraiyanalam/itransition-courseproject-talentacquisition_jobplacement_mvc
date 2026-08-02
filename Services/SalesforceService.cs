using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace talentacquisition_jobplacement_mvc.Services
{
    public class SalesforceService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public SalesforceService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<string> GetAccessTokenAsync()
        {
            var instanceUrl = _config["Salesforce:InstanceUrl"]?.TrimEnd('/');

            if (string.IsNullOrWhiteSpace(instanceUrl))
                throw new Exception("Salesforce:InstanceUrl is missing or empty.");

            var tokenUrl = $"{instanceUrl}/services/oauth2/token";

            // Ensure required config values are present (fail fast and satisfy nullable analysis)
            var consumerKey = _config["Salesforce:ConsumerKey"] ??
                              throw new Exception("Salesforce:ConsumerKey is missing or empty.");
            var consumerSecret = _config["Salesforce:ConsumerSecret"] ??
                                 throw new Exception("Salesforce:ConsumerSecret is missing or empty.");
            var username = _config["Salesforce:Username"] ??
                           throw new Exception("Salesforce:Username is missing or empty.");
            var password = _config["Salesforce:Password"] ??
                           throw new Exception("Salesforce:Password is missing or empty.");
            var securityToken = _config["Salesforce:SecurityToken"] ??
                                throw new Exception("Salesforce:SecurityToken is missing or empty.");

            // Use username-password grant (Salesforce requires this or another supported flow;
            // the client_credentials grant is not allowed by default and produced "no client credentials user enabled")
            //var content = new FormUrlEncodedContent(new Dictionary<string, string>
            //{
            //    ["grant_type"] = "password",
            //    ["client_id"] = consumerKey,
            //    ["client_secret"] = consumerSecret,
            //    ["username"] = username,
            //    ["password"] = password + securityToken
            //});
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = consumerKey,
                ["client_secret"] = consumerSecret
            });

            var response = await _httpClient.PostAsync(tokenUrl, content);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Salesforce token error: {json}");
            }

            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("access_token").GetString()!;
        }

        public async Task<(string AccountId, string ContactId)> CreateAccountAndContactAsync(
            string fullName,
            string email,
            string? company,
            string? phone,
            string? industry,
            string? title,
            string? notes)
        {
            var accessToken = await GetAccessTokenAsync();
            var instanceUrl = _config["Salesforce:InstanceUrl"]?.TrimEnd('/');
            if (string.IsNullOrWhiteSpace(instanceUrl))
                throw new Exception("Salesforce:InstanceUrl is missing or empty.");

            var apiVersion = "v59.0";

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            // 1. Create Account
            var accountPayload = new
            {
                Name = company ?? fullName + " Account",
                Industry = industry,
                Phone = phone,
                Description = notes
            };

            var accountJson = JsonSerializer.Serialize(accountPayload);
            var accountContent = new StringContent(accountJson, Encoding.UTF8, "application/json");

            var accountResponse = await _httpClient.PostAsync(
                $"{instanceUrl}/services/data/{apiVersion}/sobjects/Account",
                accountContent);

            var accountResult = await accountResponse.Content.ReadAsStringAsync();

            if (!accountResponse.IsSuccessStatusCode)
                throw new Exception($"Account creation failed: {accountResult}");

            using var accountDoc = JsonDocument.Parse(accountResult);
            string accountId = accountDoc.RootElement.GetProperty("id").GetString()!;

            // 2. Create Contact linked to Account
            var nameParts = fullName.Split(' ', 2);
            var contactPayload = new
            {
                FirstName = nameParts.Length > 0 ? nameParts[0] : fullName,
                LastName = nameParts.Length > 1 ? nameParts[1] : "Contact",
                Email = email,
                Phone = phone,
                Title = title,
                AccountId = accountId,
                Description = notes
            };

            var contactJson = JsonSerializer.Serialize(contactPayload);
            var contactContent = new StringContent(contactJson, Encoding.UTF8, "application/json");

            var contactResponse = await _httpClient.PostAsync(
                $"{instanceUrl}/services/data/{apiVersion}/sobjects/Contact",
                contactContent);

            var contactResult = await contactResponse.Content.ReadAsStringAsync();

            if (!contactResponse.IsSuccessStatusCode)
                throw new Exception($"Contact creation failed: {contactResult}");

            using var contactDoc = JsonDocument.Parse(contactResult);
            string contactId = contactDoc.RootElement.GetProperty("id").GetString()!;

            return (accountId, contactId);
        }
    }
}