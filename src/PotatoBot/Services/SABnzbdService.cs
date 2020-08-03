using Newtonsoft.Json;
using PotatoBot.Modals.API.Requests;
using PotatoBot.Modals.API.SABnzbd;
using PotatoBot.Modals.Settings;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace PotatoBot.Services
{
    public class SABnzbdService : IService
    {
        public string Name { get; }

        private const string _apiUrl = "sabnzbd/api";
        private readonly string _baseUrl;

        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        internal SABnzbdService(SABnzbdSettings settings)
        {
            Name = settings.Name;

            _baseUrl = $"{settings.Url}/{_apiUrl}?output=json&apikey={settings.APIKey}";
        }

        public bool Start()
        {
            return true;
        }

        public bool Stop()
        {
            return true;
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

        protected async Task<T> GetRequest<T>(SABnzbdRequestMode requestMode, RequestBase getRequest = null, HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            var url = $"{_baseUrl}&mode={requestMode}";

            if (getRequest != null)
            {
                url += $"&{getRequest.ToGet()}";
            }

            try
            {
                using (var client = GetHttpClient())
                {
                    _logger.Trace($"Sending request to '{url}'");

                    var response = await client.GetAsync(url);
                    if (response.StatusCode != expectedStatusCode)
                    {
                        _logger.Warn($"Unexpected Status Code: {response.StatusCode}");
                    }

                    var json = response.Content.ReadAsStringAsync().Result;
                    if (string.IsNullOrEmpty(json))
                    {
                        _logger.Warn("Empty response received");
                        return default;
                    }

                    try
                    {
                        return JsonConvert.DeserializeObject<T>(json);
                    }
                    catch(Exception ex)
                    {
                        _logger.Error(ex, "Failed to parse response");
                        return default;
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.Warn(ex, $"Failed to fetch from {url}");
                return default;
            }
        }

        internal async Task<DynamicRoot<ServerStatus>> GetStatus()
        {
            return await GetRequest<DynamicRoot<ServerStatus>>(SABnzbdRequestMode.fullstatus);
        }

        internal async Task<bool> PauseQueue()
        {
            var response = await GetRequest<DynamicRoot<bool>>(SABnzbdRequestMode.pause);
            return response.Status;
        }

        internal async Task<bool> ResumeQueue()
        {
            var response = await GetRequest<DynamicRoot<bool>>(SABnzbdRequestMode.resume);
            return response.Status;
        }

        internal async Task<DynamicRoot<SABQueue>> GetQueue(uint start = 0, uint limit = 1)
        {
            return await GetRequest<DynamicRoot<SABQueue>>(SABnzbdRequestMode.queue, new RequestSABQueue
            {
                Start = start,
                Limit = limit
            });
        }
    }
}
