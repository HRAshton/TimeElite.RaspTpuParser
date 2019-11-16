using System;
using System.Collections.Generic;
using System.Linq;
using Ical.Net;
using Newtonsoft.Json;
using RaspTpuIcalConverter.Helpers;
using RaspTpuIcalConverter.Parsers;
using RaspTpuIcalConverter.RaspTpuModels;

namespace RaspTpuIcalConverter
{
    public class RaspTruIcalConverter
    {
        private readonly UrlHelper _urlHelper;
        private readonly PageParser _pageParser;


        public RaspTruIcalConverter()
        {
            _pageParser = new PageParser();
            _urlHelper = new UrlHelper();
        }


        public Calendar GetByHtml(string pageHtml)
        {
            var result = _pageParser.ParsePage(pageHtml);

            return result;
        }

        public Calendar GetByLink(string link, byte before = 0, byte after = 0)
        {
            if (!_urlHelper.IsAbsoluteUrl(link)) link = "https://rasp.tpu.ru" + link;

            var urls = GetUrls(link, before, after);
            var mainCalendar = GetJoinedCalendarByUrls(urls);

            return mainCalendar;
        }

        public Calendar GetByQuery(string query, byte before = 0, byte after = 0)
        {
            var queryResultJson =
                _urlHelper.GetRequestContent("https://rasp.tpu.ru/select/search/main.html?q=" + query);
            var queryResult = JsonConvert.DeserializeObject<QueryResultModel>(queryResultJson);
            var trueResult =
                queryResult.Result.FirstOrDefault(x => string.Compare(x.Text, 0, query, 0, x.Text.Length, true) == 0);

            if (trueResult == null) return null;

            var calendar = GetByLink(trueResult.Url, before, after);

            return calendar;
        }


        private static RaspUrlModel ParseUrl(string url)
        {
            var fragments = url.Split('/');

            if (fragments.Length != 7) throw new Exception("Invalid rasp.tpu.ru url.");

            var result = new RaspUrlModel
            {
                Id = fragments[3],
                Year = ushort.Parse(fragments[4]),
                Week = byte.Parse(fragments[5])
            };

            return result;
        }

        private static IEnumerable<string> GetUrls(string link, byte before, byte after)
        {
            var urls = new List<string> {link};

            // ReSharper disable once InvertIf
            if (before > 0 || after > 0)
            {
                var parsedUrl = ParseUrl(link);

                for (var i = 1; i <= before; i++) urls.Add(parsedUrl.GetUrlForWeek(-i));

                for (var i = 1; i <= after; i++) urls.Add(parsedUrl.GetUrlForWeek(i));
            }

            return urls;
        }

        private Calendar GetJoinedCalendarByUrls(IEnumerable<string> urls)
        {
            var calendars = urls
                .Select(_urlHelper.GetRequestContent)
                .Select(GetByHtml)
                .ToList();

            var mainCalendar = calendars.First();
            calendars
                .Skip(1)
                .ToList()
                .ForEach(x => mainCalendar.Events.AddRange(x.Events));

            return mainCalendar;
        }
    }
}