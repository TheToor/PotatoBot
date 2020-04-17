using Newtonsoft.Json;
using PotatoBot.Modals.API;
using PotatoBot.Modals.API.Requests;
using PotatoBot.Modals.Settings;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace PotatoBot.API
{
    internal class APIBase
    {
        private readonly EntertainmentSettings _settings;
        private readonly string _apiUrl;

        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();


        internal APIBase(EntertainmentSettings settings, string apiUrl)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            if (string.IsNullOrEmpty(apiUrl))
                throw new ArgumentNullException(nameof(apiUrl));

            _settings = settings;
            _apiUrl = apiUrl;

            var systemStatus = GetSystemStatus();
            if(systemStatus == null)
            {
                throw new Exception("Test connection failed. Verify Url and API key");
            }
        }

        internal virtual SystemStatus GetSystemStatus()
        {
            return GetRequest<SystemStatus>(APIEndPoints.SystemStatus);
        }

        internal virtual List<QualityProfile> GetQualityProfiles()
        {
            return GetRequest<List<QualityProfile>>(APIEndPoints.QualityProfile);
        }

        internal virtual List<LanguageProfile> GetLanguageProfiles()
        {
            return GetRequest<List<LanguageProfile>>(APIEndPoints.LanguageProfile);
        }

        private HttpClient GetHttpClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );
            client.DefaultRequestHeaders.Add("User-Agent", Program.Namespace);
            return client;
        }

        protected T GetRequest<T>(string endpoint, RequestBase getRequest = null, HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            var client = GetHttpClient();
            var url = $"{_settings.Url}/{_apiUrl}/{endpoint}?apikey={_settings.APIKey}";

            if (getRequest != null)
            {
                url += $"&{getRequest.ToGet()}";
            }

            try
            {
                _logger.Trace($"Sending request to '{url}'");

                var response = client.GetAsync(url).Result;
                if(response.StatusCode != expectedStatusCode)
                {
                    _logger.Warn($"Unexpected Status Code: {response.StatusCode}");
                }

                var json = response.Content.ReadAsStringAsync().Result;
                if(string.IsNullOrEmpty(json))
                {
                    _logger.Warn("Empty response received");
                    return default(T);
                }

                return JsonConvert.DeserializeObject<T>(json);
            }
            catch(Exception ex)
            {
                _logger.Error(ex, $"Failed to process request to {endpoint}");
                return default(T);
            }
        }

        protected T PostRequest<T>(string endpoint, object body, HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            var client = GetHttpClient();
            var url = $"{_settings.Url}/{_apiUrl}/{endpoint}?apikey={_settings.APIKey}";

            try
            {
                _logger.Trace($"Sending request to '{url}'");

                var serialized = JsonConvert.SerializeObject(body);
                var content = new StringContent(serialized, Encoding.UTF8, "application/json");
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                _logger.Trace($"Content: {content.ReadAsStringAsync().Result}");

                var response = client.PostAsync(url, content).Result;
                if (response.StatusCode != expectedStatusCode)
                {
                    _logger.Warn($"Unexpected Status Code: {response.StatusCode}");
                }

                var json = response.Content.ReadAsStringAsync().Result;
                if (string.IsNullOrEmpty(json))
                {
                    _logger.Warn("Empty response received");
                    return default(T);
                }

                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to process request to {endpoint}");
                return default(T);
            }
        }
    }
}
