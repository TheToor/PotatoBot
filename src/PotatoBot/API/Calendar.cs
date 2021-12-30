using Ical.Net.Serialization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace PotatoBot.API
{
    [Route("[controller]", Name = "Calendar")]
    public class Calendar : Controller
    {
        internal static readonly List<APIBase> Calendars = new List<APIBase>();

        private static DateTime _lastUpdate;
        private static byte[] _cachedCalendar;

        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        [Route("")]
        [HttpGet]
        public IActionResult Index()
        {
            if(_cachedCalendar != null && DateTime.Now < _lastUpdate.AddMinutes(30))
            {
                _logger.Trace("Using cached calendar");
                return File(_cachedCalendar, "text/calendar", "event.ics");
            }

            var combinedCalendar = new Ical.Net.Calendar();

            foreach(var api in Calendars)
            {
                var calendar = api.GetCalendar();

                if(calendar == null)
                {
                    _logger.Debug($"Failed to get calendar for {api.GetType().Name}");
                    continue;
                }

                combinedCalendar.Events.AddRange(calendar.Events);
            }

            _logger.Trace($"Total of {combinedCalendar.Events.Count} events");

            var serializer = new CalendarSerializer(new SerializationContext());
            var serializedCalendar = serializer.SerializeToString(combinedCalendar);
            var bytesCalendar = Encoding.UTF8.GetBytes(serializedCalendar);

            _cachedCalendar = bytesCalendar;
            _lastUpdate = DateTime.Now;

            return File(bytesCalendar, "text/calendar", "event.ics");
        }
    }
}
