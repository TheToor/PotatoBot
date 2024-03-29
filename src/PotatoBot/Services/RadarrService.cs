﻿using PotatoBot.Controllers;
using PotatoBot.Model;
using PotatoBot.Model.API;
using PotatoBot.Model.API.Radarr;
using PotatoBot.Model.API.Requests;
using PotatoBot.Model.API.Requests.POST;
using PotatoBot.Model.API.Servarr;
using PotatoBot.Model.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PotatoBot.Services
{
    public class RadarrService : APIBase, IService, IServarr, IServarrSupportsDiscovery
    {
        public ServarrType Type => ServarrType.Radarr;

        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        internal RadarrService(EntertainmentSettings settings, string apiUrl) : base(settings, apiUrl)
        {
            var systemStatus = GetSystemStatus();
            _logger.Trace("======= System Info =======");
            _logger.Trace($"Version: {systemStatus.Version}");
            _logger.Trace($"OS: {systemStatus.OSVersion}");
            _logger.Trace($"IsMono: {systemStatus.IsMono}");
            _logger.Trace($"IsLinux: {systemStatus.IsLinux}");
            _logger.Trace($"IsWindows: {systemStatus.IsWindows}");
        }

        public bool Start()
        {
            return true;
        }

        public bool Stop()
        {
            return true;
        }

        public IEnumerable<IServarrItem> GetAll()
        {
            _logger.Trace("Fetching all movies ...");

            var response = GetRequest<List<Movie>>(APIEndPoints.RadarrEndpoints.Movie);
            _logger.Trace($"Got {response.Count} movies as a response");

            return response;
        }

        public IServarrItem GetById(ulong id)
        {
            _logger.Trace($"Fetching {id} ...");

            var response = GetRequest<Movie>($"{APIEndPoints.RadarrEndpoints.Movie}/{id}", failOnUnexpectedStatusCode: true);
            return response;
        }

        public IEnumerable<IServarrItem> Search(string name)
        {
            _logger.Trace($"Searching movie with name {name} ...");
            var requestBody = new LookupRequest()
            {
                SearchTerm = name
            };

            var response = GetRequest<List<Movie>>(APIEndPoints.RadarrEndpoints.Lookup, requestBody);
            _logger.Trace($"Got {response.Count} movies as response");

            return response;
        }

        public AddResult Add(IServarrItem item)
        {
            var movie = item as Movie ?? throw new ArgumentNullException(nameof(item));

            _logger.Trace($"Adding movie [{movie.TMDBId}] {movie.Title}");

            var postBody = new AddMovie(this, movie);

            var response = PostRequest<Movie>(APIEndPoints.RadarrEndpoints.Movie, postBody, System.Net.HttpStatusCode.Created);
            if(response.Item1 != null)
            {
                var movieResponse = response.Item1;
                _logger.Trace($"Successfully added {movieResponse.Title} ({movieResponse.Path})");
            }
            return new AddResult
            {
                Added = response.Item1 != null,
                AlreadyAdded = response.Item2 == System.Net.HttpStatusCode.BadRequest,
                StatusCode = System.Net.HttpStatusCode.BadRequest
            };
        }

        public override List<QueueItem> GetQueue()
        {
            _logger.Trace("Fetching download queue");

            var response = GetRequest<Model.API.Queue<RadarrQueueItem>>(APIEndPoints.Queue);
            if(response != null)
            {
                _logger.Trace("Successfully fetched download queue");
            }

            var list = new List<QueueItem>();
            foreach(var record in response.Records)
            {
                record.API = this;
                list.Add(record);
            }
            return list;
        }

        public IEnumerable<IServarrItem> GetDiscoveryQueue()
        {
            _logger.Trace("Fetching discovery queue");

            var response = GetRequest<List<Movie>>(APIEndPoints.RadarrEndpoints.ImportList, new ImportList());
            if(response != null)
            {
                _logger.Trace($"Fetched {response.Count} movies as response. Filtering");
                var filtered = response.Where(m => !m.IsExcluded && !m.IsExisting && m.IsRecommendation);
                _logger.Trace($"Returning {filtered.Count()} filtered movies");
                return filtered;
            }
            return null;
        }
    }
}
