using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using HRAshton.TimeElite.RaspTpuParser.Helpers;
using HRAshton.TimeElite.RaspTpuParser.Parsers;
using HRAshton.TimeElite.RaspTpuParser.RaspTpuModels;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace HRAshton.TimeElite.RaspTpuParser
{
    /// <summary>
    /// Модуль парсинга расписания.
    /// </summary>
    public class RaspTpuIcalConverter
    {
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="httpClient">Клиент для запросов (с прокси-сервером, если надо).</param>
        /// <param name="memoryCache">Кэш.</param>
        /// <param name="cacheTime">Время жизни кэша.</param>
        public RaspTpuIcalConverter(
            HttpClient httpClient = null,
            IMemoryCache memoryCache = null,
            TimeSpan cacheTime = default)
        {
            MemoryCache = memoryCache ?? new MemoryCache(new MemoryCacheOptions());
            PageParser = new PageParser();
            UrlHelper = new UrlHelper(httpClient ?? new HttpClient(), MemoryCache);
            CacheTime = cacheTime == default
                ? TimeSpan.FromMilliseconds(1)
                : cacheTime;
        }

        private IMemoryCache MemoryCache { get; }

        private PageParser PageParser { get; }

        private UrlHelper UrlHelper { get; }

        private TimeSpan CacheTime { get; }

        /// <summary>
        /// Получает страницу по url и преобразует строку с html-кодом страницы
        /// в объект типа <see cref="CalendarWithTimesModel" /> (Ical.Net).
        /// <br />
        /// У календаря заполняются поля Name и Events, содержащее события. У события заполняются только атрибуты:
        /// <list type="bullet">
        ///     <item> Name </item>
        ///     <item> Categories </item>
        ///     <item> Contacts </item>
        ///     <item> Location </item>
        ///     <item> Description </item>
        ///     <item> DtStart </item>
        ///     <item> DtEnd </item>
        ///     <item> Duration </item>
        /// </list>
        /// </summary>
        /// <param name="pageHtml">Строка с html-кодом страницы.</param>
        /// <returns>Календарь с названием и событиями.</returns>
        public CalendarWithTimesModel GetByHtml(string pageHtml)
        {
            var result = PageParser.ParsePage(pageHtml);

            return result;
        }

        /// <summary>
        /// Получает страницу по url и преобразует строку с html-кодом страницы
        /// в объект типа <see cref="CalendarWithTimesModel" /> (Ical.Net).
        /// <br />
        /// У календаря заполняются поля Name и Events, содержащее события. У события заполняются только атрибуты:
        /// <list type="bullet">
        ///     <item> Name </item>
        ///     <item> Categories </item>
        ///     <item> Contacts </item>
        ///     <item> Location </item>
        ///     <item> Description </item>
        ///     <item> DtStart </item>
        ///     <item> DtEnd </item>
        ///     <item> Duration </item>
        /// </list>
        /// </summary>
        /// <param name="link">Url получаемой страницы.</param>
        /// <returns>Календарь с названием и событиями.</returns>
        public CalendarWithTimesModel GetByLink(string link)
        {
            var mainCalendar = GetByLink(link, 0, 0, 0);

            return mainCalendar;
        }

        /// <summary>
        /// Получает страницу по url и преобразует строку с html-кодом страницы
        /// в объект типа <see cref="CalendarWithTimesModel" /> (Ical.Net).
        /// <br />
        /// У календаря заполняются поля Name и Events, содержащее события. У события заполняются только атрибуты:
        /// <list type="bullet">
        ///     <item> Name </item>
        ///     <item> Categories </item>
        ///     <item> Contacts </item>
        ///     <item> Location </item>
        ///     <item> Description </item>
        ///     <item> DtStart </item>
        ///     <item> DtEnd </item>
        ///     <item> Duration </item>
        /// </list>
        /// </summary>
        /// <param name="link">Url получаемой страницы.</param>
        /// <param name="before">Количество календарей до переданного, события которых нужно включить.</param>
        /// <param name="skipBetweenCurrentWeekAndAfter">Количество недель (включая текущую), которые должны быть пропущены.</param>
        /// <param name="after">Количество календарей после переданного, события которых нужно включить.</param>
        /// <returns>Календарь с названием и событиями.</returns>
        public CalendarWithTimesModel GetByLink(
            string link,
            byte before,
            byte skipBetweenCurrentWeekAndAfter,
            byte after)
        {
            if (!UrlHelper.IsAbsoluteUrl(link))
            {
                link = "https://rasp.tpu.ru" + link;
            }

            var urls = GetUrls(link, before, skipBetweenCurrentWeekAndAfter, after);
            var mainCalendar = GetJoinedCalendarByUrls(urls);

            return mainCalendar;
        }

        /// <summary>
        /// Получает страницу по идентификатору (hash) и возвращает расписание -
        /// объект типа <see cref="CalendarWithTimesModel" /> (Ical.Net).
        /// <br />
        /// У календаря заполняются поля Name и Events, содержащее события. У события заполняются только атрибуты:
        /// <list type="bullet">
        ///     <item> Name </item>
        ///     <item> Categories </item>
        ///     <item> Contacts </item>
        ///     <item> Location </item>
        ///     <item> Description </item>
        ///     <item> DtStart </item>
        ///     <item> DtEnd </item>
        ///     <item> Duration </item>
        /// </list>
        /// </summary>
        /// <param name="hash">Хэш-код расписания.</param>
        /// <returns>Календарь с названием и событиями.</returns>
        public CalendarWithTimesModel GetByHash(string hash)
        {
            var mainCalendar = GetByHash(hash, 0, 0, 0);

            return mainCalendar;
        }

        /// <summary>
        /// Получает страницу по hash и возвращает расписание - объект типа <see cref="CalendarWithTimesModel" />
        /// (Ical.Net).
        /// <br />
        /// У календаря заполняются поля Name и Events, содержащее события. У события заполняются только атрибуты:
        /// <list type="bullet">
        ///     <item> Name </item>
        ///     <item> Categories </item>
        ///     <item> Contacts </item>
        ///     <item> Location </item>
        ///     <item> Description </item>
        ///     <item> DtStart </item>
        ///     <item> DtEnd </item>
        ///     <item> Duration </item>
        /// </list>
        /// </summary>
        /// <param name="hash">Идентификатор (hash) требуемой страницы.</param>
        /// <param name="before">Количество календарей до переданного, события которых нужно включить.</param>
        /// <param name="skipBetweenCurrentWeekAndAfter">
        /// Количество недель (включая текущую), которые должны быть пропущены.
        /// </param>
        /// <param name="after">Количество календарей после переданного, события которых нужно включить.</param>
        /// <returns>Календарь с названием и событиями.</returns>
        public CalendarWithTimesModel GetByHash(
            string hash,
            byte before,
            byte skipBetweenCurrentWeekAndAfter,
            byte after)
        {
            var hashedLink = "https://rasp.tpu.ru/redirect/kalendar.html?hash=" + hash;
            var trueUrl = UrlHelper.GetFinalRedirect(hashedLink);
            var urls = GetUrls(trueUrl, before, skipBetweenCurrentWeekAndAfter, after);
            var mainCalendar = GetJoinedCalendarByUrls(urls);

            return mainCalendar;
        }

        /// <summary>
        /// Получает страницу по точному совпадению в поиске (<paramref name="query" />)
        /// и преобразует строку с html-кодом страницы в объект типа <see cref="CalendarWithTimesModel" /> (Ical.Net).
        /// <br />
        /// У календаря заполняются поля Name и Events, содержащее события. У события заполняются только атрибуты:
        /// <list type="bullet">
        ///     <item> Name </item>
        ///     <item> Categories </item>
        ///     <item> Contacts </item>
        ///     <item> Location </item>
        ///     <item> Description </item>
        ///     <item> DtStart </item>
        ///     <item> DtEnd </item>
        ///     <item> Duration </item>
        /// </list>
        /// </summary>
        /// <param name="query">
        /// Текст поиска (можно проверить на основной странице rasp.tpu.ru), первое точное совпадение которого
        /// учитывается.
        /// </param>
        /// <returns>Календарь с названием и событиями.</returns>
        public CalendarWithTimesModel GetByQuery(string query)
        {
            var calendar = GetByQuery(query, 0, 0, 0);

            return calendar;
        }

        /// <summary>
        /// Получает страницу по точному совпадению в поиске (<paramref name="query" />) и преобразует строку
        /// с html-кодом страницы в объект типа <see cref="CalendarWithTimesModel" /> (Ical.Net).
        /// <br />
        /// У календаря заполняются поля Name и Events, содержащее события. У события заполняются только атрибуты:
        /// <list type="bullet">
        ///     <item> Name </item>
        ///     <item> Categories </item>
        ///     <item> Contacts </item>
        ///     <item> Location </item>
        ///     <item> Description </item>
        ///     <item> DtStart </item>
        ///     <item> DtEnd </item>
        ///     <item> Duration </item>
        /// </list>
        /// </summary>
        /// <param name="query">
        /// Текст поиска (можно проверить на основной странице rasp.tpu.ru), первое точное совпадение которого
        /// учитывается.
        /// </param>
        /// <param name="before">Количество календарей до переданного, события которых нужно включить.</param>
        /// <param name="skipBetweenCurrentWeekAndAfter">
        /// Количество недель (включая текущую), которые должны быть пропущены.
        /// </param>
        /// <param name="after">Количество календарей после переданного, события которых нужно включить.</param>
        /// <returns>Календарь с названием и событиями.</returns>
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

            var hashedLink = "https://rasp.tpu.ru" + trueResult.Url;
            var trueUrl = UrlHelper.GetFinalRedirect(hashedLink);

            var calendar = GetByLink(trueUrl, before, skipBetweenCurrentWeekAndAfter, after);

            return calendar;
        }

        /// <summary>
        /// Получает результат поиска на сайте rasp.tpu.ru аналогично полю ввода на главной странице.
        /// </summary>
        /// <param name="query">Запрос для поиска.</param>
        /// <returns>Перечисление результатов.</returns>
        public IEnumerable<QueryResultItemModel> GetSearchResults(string query)
        {
            var queryUrl = "https://rasp.tpu.ru/select/search/main.html?q=" + query;
            var queryResultJson = UrlHelper.GetRequestContent(queryUrl, CacheTime);
            var queryResult = JsonConvert.DeserializeObject<QueryResultModel>(queryResultJson);
            var result = queryResult.Result;

            return result;
        }

        private static RaspUrlModel ParseUrl(string url)
        {
            var fragments = url.Split('/');

            if (fragments.Length != 7)
            {
                throw new Exception("Invalid rasp.tpu.ru url.");
            }

            var result = new RaspUrlModel
            {
                Id = fragments[3],
                Year = ushort.Parse(fragments[4]),
                Week = byte.Parse(fragments[5]),
            };

            return result;
        }

        private static IEnumerable<string> GetUrls(
            string link,
            byte before,
            byte skipBetweenCurrentWeekAndAfter,
            byte after)
        {
            var urls = new List<string> { link };

            // ReSharper disable once InvertIf
            if (before > 0 || after > 0 || skipBetweenCurrentWeekAndAfter > 0)
            {
                var parsedUrl = ParseUrl(link);

                for (var i = 1; i <= before; i++)
                {
                    urls.Add(parsedUrl.GetUrlForWeek(-i));
                }

                for (var i = skipBetweenCurrentWeekAndAfter + 1; i <= after; i++)
                {
                    urls.Add(parsedUrl.GetUrlForWeek(i));
                }
            }

            return urls;
        }

        private CalendarWithTimesModel GetJoinedCalendarByUrls(IEnumerable<string> urls)
        {
            var calendars = urls
                .Select(x => UrlHelper.GetRequestContent(x, CacheTime))
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