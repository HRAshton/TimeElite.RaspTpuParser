using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
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
        public Task<CalendarWithTimesModel> GetByLinkAsync(string link)
        {
            return GetByLinkAsync(link, 0, 0, 0);
        }

        /// <inheritdoc/>
        public async Task<CalendarWithTimesModel> GetByLinkAsync(
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
            var mainCalendar = await GetJoinedCalendarByUrlsAsync(urls);

            return mainCalendar;
        }

        /// <inheritdoc/>
        public Task<CalendarWithTimesModel> GetByHashAsync(string hash)
        {
            return GetByHashAsync(hash, 0, 0, 0);
        }

        /// <inheritdoc/>
        public async Task<CalendarWithTimesModel> GetByHashAsync(
            string hash,
            byte before,
            byte skipBetweenCurrentWeekAndAfter,
            byte after)
        {
            var hashedLink = RaspTpuUrlHelper.CreateFromHash(hash);
            var trueUrl = await HttpClient.GetFinalRedirectAsync(hashedLink);
            var urls = UrlHelper.CreateUrls(trueUrl, before, skipBetweenCurrentWeekAndAfter, after);
            var mainCalendar = await GetJoinedCalendarByUrlsAsync(urls);

            return mainCalendar;
        }

        /// <inheritdoc/>
        public Task<CalendarWithTimesModel> GetByQueryAsync(string query)
        {
            return GetByQueryAsync(query, 0, 0, 0);
        }

        /// <inheritdoc/>
        public async Task<CalendarWithTimesModel> GetByQueryAsync(
            string query,
            byte before,
            byte skipBetweenCurrentWeekAndAfter,
            byte after)
        {
            var searchResults = await GetSearchResultsAsync(query);

            var trueResult = searchResults
                .FirstOrDefault(item => string.Compare(item.Text, 0, query, 0, item.Text.Length, true) == 0);
            if (trueResult == null)
            {
                return null;
            }

            var hashedLink = RaspTpuUrlHelper.CreateFromRelationPath(trueResult.Url);
            var trueUrl = await HttpClient.GetFinalRedirectAsync(hashedLink);

            var calendar = await GetByLinkAsync(trueUrl, before, skipBetweenCurrentWeekAndAfter, after);

            return calendar;
        }

        /// <inheritdoc/>
        public async Task<SearchResultItemModel[]> GetSearchResultsAsync(string query)
        {
            var queryUrl = RaspTpuUrlHelper.CreateQuery(query);
            var queryResultJson = await HttpClient.GetStringAsync(queryUrl);
            var queryResult = JsonConvert.DeserializeObject<SearchResultModel>(queryResultJson);
            var result = queryResult.Result;

            return result;
        }

        private async Task<CalendarWithTimesModel> GetJoinedCalendarByUrlsAsync(IEnumerable<string> urls)
        {
            var calendars = await FetchCalendarsAsync(urls);

            var mainCalendar = MergeCalendars(calendars);

            return mainCalendar;
        }

        private async Task<CalendarWithTimesModel[]> FetchCalendarsAsync(IEnumerable<string> urls)
        {
            var tasks = urls
                .Select(GetByHtmlAsync)
                .ToArray();

            await Task.WhenAll(tasks);

            var calendars = tasks
                .Select(task => task.Result)
                .ToArray();

            return calendars;
        }

        private static CalendarWithTimesModel MergeCalendars(CalendarWithTimesModel[] calendars)
        {
            var mainCalendar = calendars.First();
            foreach (var secondaryCalendar in calendars.Skip(1))
            {
                mainCalendar.Events.AddRange(secondaryCalendar.Events);
            }

            return mainCalendar;
        }

        private async Task<CalendarWithTimesModel> GetByHtmlAsync(string url)
        {
            using var stream = await HttpClient.GetStreamAsync(url);

            var htmlDocument = new HtmlDocument();
            htmlDocument.Load(stream);

            await RaspTpuDecryptor.DecryptAllAsync(htmlDocument);

            var calendarWithTimesModel = PageParser.ParsePage(htmlDocument);

            return calendarWithTimesModel;
        }
    }
}