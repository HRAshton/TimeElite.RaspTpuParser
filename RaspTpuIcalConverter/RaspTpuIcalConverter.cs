using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using HRAshton.TimeElite.RaspTpuParser.Extensions;
using HRAshton.TimeElite.RaspTpuParser.Helpers;
using HRAshton.TimeElite.RaspTpuParser.Models;
using HRAshton.TimeElite.RaspTpuParser.Parsers;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace HRAshton.TimeElite.RaspTpuParser
{
    /// <summary>
    /// Модуль парсинга расписания.
    /// </summary>
    internal class RaspTpuIcalConverter : IRaspTpuIcalConverter
    {
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="httpClient">Клиент для запросов (с прокси-сервером, если надо).</param>
        /// <param name="pageParser">Парсер для html-страницы сайта rasp.tpu.ru.</param>
        /// <param name="raspTpuDecryptor">Дешифратор зашифрованных XOR-ом тегов (с названиями пар).</param>
        public RaspTpuIcalConverter(HttpClient httpClient, PageParser pageParser, RaspTpuDecryptor raspTpuDecryptor)
        {
            HttpClient = httpClient;
            PageParser = pageParser;
            RaspTpuDecryptor = raspTpuDecryptor;
        }

        private PageParser PageParser { get; }

        private HttpClient HttpClient { get; }

        private RaspTpuDecryptor RaspTpuDecryptor { get; }

        /// <inheritdoc/>
        public CalendarWithTimesModel GetByHtml(string pageHtml)
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(pageHtml);

            RaspTpuDecryptor.DecryptAll(htmlDocument);

            var calendarWithTimesModel = PageParser.ParsePage(htmlDocument);

            return calendarWithTimesModel;
        }

        /// <inheritdoc/>
        public CalendarWithTimesModel GetByLink(string link)
        {
            var mainCalendar = GetByLink(link, 0, 0, 0);

            return mainCalendar;
        }

        /// <inheritdoc/>
        public CalendarWithTimesModel GetByLink(
            string link,
            byte before,
            byte skipBetweenCurrentWeekAndAfter,
            byte after)
        {
            if (!UrlHelper.IsAbsoluteUrl(link))
            {
                link = RaspTpuUrlHelper.CreateFromRelationPath(link);
            }

            var urls = UrlHelper.CreateUrls(link, before, skipBetweenCurrentWeekAndAfter, after);
            var mainCalendar = GetJoinedCalendarByUrls(urls);

            return mainCalendar;
        }

        /// <inheritdoc/>
        public CalendarWithTimesModel GetByHash(string hash)
        {
            var mainCalendar = GetByHash(hash, 0, 0, 0);

            return mainCalendar;
        }

        /// <inheritdoc/>
        public CalendarWithTimesModel GetByHash(
            string hash,
            byte before,
            byte skipBetweenCurrentWeekAndAfter,
            byte after)
        {
            var hashedLink = RaspTpuUrlHelper.CreateFromHash(hash);
            var trueUrl = HttpClient.GetFinalRedirect(hashedLink);
            var urls = UrlHelper.CreateUrls(trueUrl, before, skipBetweenCurrentWeekAndAfter, after);
            var mainCalendar = GetJoinedCalendarByUrls(urls);

            return mainCalendar;
        }

        /// <inheritdoc/>
        public CalendarWithTimesModel GetByQuery(string query)
        {
            var calendar = GetByQuery(query, 0, 0, 0);

            return calendar;
        }

        /// <inheritdoc/>
        public CalendarWithTimesModel GetByQuery(
            string query,
            byte before,
            byte skipBetweenCurrentWeekAndAfter,
            byte after)
        {
            var searchResults = GetSearchResults(query);

            var trueResult = searchResults
                .FirstOrDefault(item => string.Compare(item.Text, 0, query, 0, item.Text.Length, true) == 0);
            if (trueResult == null)
            {
                return null;
            }

            var hashedLink = RaspTpuUrlHelper.CreateFromRelationPath(trueResult.Url);
            var trueUrl = HttpClient.GetFinalRedirect(hashedLink);

            var calendar = GetByLink(trueUrl, before, skipBetweenCurrentWeekAndAfter, after);

            return calendar;
        }

        /// <inheritdoc/>
        public SearchResultItemModel[] GetSearchResults(string query)
        {
            var queryUrl = RaspTpuUrlHelper.CreateQuery(query);
            var queryResultJson = HttpClient.GetRequestContent(queryUrl);
            var queryResult = JsonConvert.DeserializeObject<SearchResultModel>(queryResultJson);
            var result = queryResult.Result;

            return result;
        }

        private CalendarWithTimesModel GetJoinedCalendarByUrls(IEnumerable<string> urls)
        {
            var calendars = urls
                .Select(HttpClient.GetRequestContent)
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