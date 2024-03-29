﻿using Newtonsoft.Json;
using PotatoBot.Model.API;
using PotatoBot.Model.API.Requests;
using PotatoBot.Model.API.Requests.DELETE;
using PotatoBot.Model.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace PotatoBot.Controllers
{
    public abstract class APIBase
    {
        public string Name => _settings.Name ?? "Unknown";
        public EntertainmentSettings Settings => _settings;

        private readonly EntertainmentSettings _settings;
        private readonly string _apiUrl;

        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        internal APIBase(EntertainmentSettings settings, string apiUrl)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _apiUrl = apiUrl ?? throw new ArgumentNullException(nameof(apiUrl));

            var systemStatus = GetSystemStatus();
            if(systemStatus == null)
            {
                throw new Exception("Test connection failed. Verify Url and API key");
            }
        }

        internal SystemStatus GetSystemStatus()
        {
            return GetRequest<SystemStatus>(APIEndPoints.SystemStatus);
        }

        internal virtual List<DiskSpace> GetDiskSpace()
        {
            return GetRequest<List<DiskSpace>>(APIEndPoints.DiskSpace);
        }

        internal virtual List<QualityProfile> GetQualityProfiles()
        {
            return GetRequest<List<QualityProfile>>(APIEndPoints.QualityProfile);
        }

        internal virtual List<LanguageProfile> GetLanguageProfiles()
        {
            return GetRequest<List<LanguageProfile>>(APIEndPoints.LanguageProfile);
        }

        public abstract List<QueueItem> GetQueue();

        internal virtual bool RemoveFromQueue(RemoveQueueItem request)
        {
            return DeleteRequest($"{APIEndPoints.Queue}/{request.Id}", request);
        }

        private static HttpClient GetHttpClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );
            client.DefaultRequestHeaders.Add("User-Agent", Program.Namespace);
            return client;
        }

        protected bool DeleteRequest(string endpoint, RequestBase? getRequest = null, HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            var url = $"{_settings.Url}/{_apiUrl}/{endpoint}?apikey={_settings.APIKey}";
            if(getRequest != null)
            {
                url += $"&{getRequest.ToGet()}";
            }

            using var client = GetHttpClient();
            try
            {
                _logger.Trace($"Sending request to '{url}'");

                var response = client.DeleteAsync(url).Result;
                if(response.StatusCode != expectedStatusCode)
                {
                    _logger.Warn($"Unexpected Status Code: {response.StatusCode}");
                    return false;
                }
                return true;
            }
            catch(Exception ex)
            {
                _logger.Error(ex, $"Failed to process request to {endpoint}");
                return false;
            }
        }

        protected T GetRequest<T>(string endpoint, RequestBase? getRequest = null, HttpStatusCode expectedStatusCode = HttpStatusCode.OK, bool failOnUnexpectedStatusCode = false) where T : class
        {
            var url = $"{_settings.Url}/{_apiUrl}/{endpoint}?apikey={_settings.APIKey}";
            if(getRequest != null)
            {
                url += $"&{getRequest.ToGet()}";
            }

            using var client = GetHttpClient();
            try
            {
                _logger.Trace($"Sending request to '{url}'");

                var response = client.GetAsync(url).Result;
                if(response.StatusCode != expectedStatusCode)
                {
                    _logger.Warn($"Unexpected Status Code: {response.StatusCode}");
                    if(failOnUnexpectedStatusCode)
                    {
                        return null;
                    }
                }

                var json = response.Content.ReadAsStringAsync().Result;
                if(string.IsNullOrEmpty(json))
                {
                    _logger.Warn("Empty response received");
                    return null;
                }

                return JsonConvert.DeserializeObject<T>(json);
            }
            catch(Exception ex)
            {
                _logger.Error(ex, $"Failed to process request to {endpoint}");
                return null;
            }
        }

        protected Tuple<T, HttpStatusCode> PostRequest<T>(string endpoint, object body, params HttpStatusCode[] expectedStatusCode)
        {
            var url = $"{_settings.Url}/{_apiUrl}/{endpoint}?apikey={_settings.APIKey}";

            using var client = GetHttpClient();
            try
            {
                _logger.Trace($"Sending request to '{url}'");

                var serialized = JsonConvert.SerializeObject
                (
                    body,
                    Formatting.None,
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    }
                );
                var content = new StringContent(serialized, Encoding.UTF8, "application/json");
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                _logger.Trace($"Content: {content.ReadAsStringAsync().Result}");

                var response = client.PostAsync(url, content).Result;
                if(!expectedStatusCode.Contains(response.StatusCode))
                {
                    // Do not attempt to deserialize an unknown status code
                    _logger.Warn($"Unexpected Status Code: {response.StatusCode}");
                    return new Tuple<T, HttpStatusCode>(default, response.StatusCode);
                }

                var json = response.Content.ReadAsStringAsync().Result;
                if(string.IsNullOrEmpty(json))
                {
                    _logger.Warn("Empty response received");
                    return new Tuple<T, HttpStatusCode>(default, HttpStatusCode.NoContent);
                }

                try
                {
                    return new Tuple<T, HttpStatusCode>(JsonConvert.DeserializeObject<T>(json), response.StatusCode);
                }
                catch(Exception ex)
                {
                    _logger.Warn(ex, "Failed to deserialize response. See content after stack trace");
                    _logger.Warn($"JSON: {json}");

                    return new Tuple<T, HttpStatusCode>(default, response.StatusCode);
                }
            }
            catch(Exception ex)
            {
                _logger.Error(ex, $"Failed to process request to {endpoint}");
                return new Tuple<T, HttpStatusCode>(default, HttpStatusCode.InternalServerError);
            }
        }

        internal Ical.Net.Calendar GetCalendar()
        {
            if(string.IsNullOrEmpty(_settings.CalendarOptions))
            {
                _logger.Debug("Cannot get calendar as calendar url is empty!");
                return null;
            }

            _logger.Trace("Getting calendar ...");

            try
            {
                var url = $"{_settings.Url}/feed/calendar/{_settings.CalendarOptions}&apikey={_settings.APIKey}";
                using var client = GetHttpClient();
                var response = client.GetAsync(url).Result;

                if(response.StatusCode == HttpStatusCode.OK)
                {
                    var text = response.Content.ReadAsStringAsync().Result;
                    if(string.IsNullOrEmpty(text))
                    {
                        _logger.Debug("Empty calendar!");
                        return null;
                    }

                    return Ical.Net.Calendar.Load(text);
                }
            }
            catch(Exception ex)
            {
                _logger.Error(ex, "Failed to get Calendar");
            }
            return null;
        }
    }
}
