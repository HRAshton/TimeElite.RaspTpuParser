using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Ical.Net;
using Newtonsoft.Json;
using RaspTpuIcalConverter.Helpers;
using RaspTpuIcalConverter.Parsers;
using RaspTpuIcalConverter.RaspTpuModels;

namespace RaspTpuIcalConverter
{
    public class RaspTruIcalConverter
    {
        private readonly PageParser _pageParser;
        private readonly UrlHelper _urlHelper;

        /// <summary>
        ///     Конструктор.
        /// </summary>
        public RaspTruIcalConverter(HttpClient httpClient = null)
        {
            _pageParser = new PageParser();
            _urlHelper = new UrlHelper(httpClient ?? new HttpClient());
        }

        /// <summary>
        ///     Получает страницу по url и преобразует строку с html-кодом страницы в объект типа <see cref="Calendar" />
        ///     (Ical.Net).
        ///     У календаря заполняются поля Name и Events, содержащее события. У события заполняются только атрибуты:
        ///     <list type="bullet">
        ///         <item> Name </item>
        ///         <item> Categories </item>
        ///         <item> Contacts </item>
        ///         <item> Location </item>
        ///         <item> Description </item>
        ///         <item> DtStart </item>
        ///         <item> DtEnd </item>
        ///         <item> Duration </item>
        ///     </list>
        /// </summary>
        /// <param name="pageHtml">Строка с html-кодом страницы.</param>
        /// <returns>Календарь с названием и событиями.</returns>
        public Calendar GetByHtml(string pageHtml)
        {
            var result = _pageParser.ParsePage(pageHtml);

            return result;
        }

        /// <summary>
        ///     Получает страницу по url и преобразует строку с html-кодом страницы в объект типа <see cref="Calendar" />
        ///     (Ical.Net).
        ///     У календаря заполняются поля Name и Events, содержащее события. У события заполняются только атрибуты:
        ///     <list type="bullet">
        ///         <item> Name </item>
        ///         <item> Categories </item>
        ///         <item> Contacts </item>
        ///         <item> Location </item>
        ///         <item> Description </item>
        ///         <item> DtStart </item>
        ///         <item> DtEnd </item>
        ///         <item> Duration </item>
        ///     </list>
        /// </summary>
        /// <param name="link">Url получаемой страницы.</param>
        /// <param name="before">Количество календарей до переданного, события которых нужно включить.</param>
        /// <param name="after">Количество календарей после переданного, события которых нужно включить.</param>
        /// <returns>Календарь с названием и событиями.</returns>
        public Calendar GetByLink(string link, byte before = 0, byte after = 0)
        {
            if (!_urlHelper.IsAbsoluteUrl(link)) link = "https://rasp.tpu.ru" + link;

            var urls = GetUrls(link, before, after);
            var mainCalendar = GetJoinedCalendarByUrls(urls);

            return mainCalendar;
        }

        /// <summary>
        ///     Получает страницу по точному совпадению в поиске (<paramref name="query" />) и преобразует строку с html-кодом
        ///     страницы в объект типа <see cref="Calendar" /> (Ical.Net).
        ///     У календаря заполняются поля Name и Events, содержащее события. У события заполняются только атрибуты:
        ///     <list type="bullet">
        ///         <item> Name </item>
        ///         <item> Categories </item>
        ///         <item> Contacts </item>
        ///         <item> Location </item>
        ///         <item> Description </item>
        ///         <item> DtStart </item>
        ///         <item> DtEnd </item>
        ///         <item> Duration </item>
        ///     </list>
        /// </summary>
        /// <param name="query">
        ///     Текст поиска (можно проверить на основной странице rasp.tpu.ru), первое точное совпадение которого
        ///     учитывается.
        /// </param>
        /// <param name="before">Количество календарей до переданного, события которых нужно включить.</param>
        /// <param name="after">Количество календарей после переданного, события которых нужно включить.</param>
        /// <returns>Календарь с названием и событиями.</returns>
        public Calendar GetByQuery(string query, byte before = 0, byte after = 0)
        {
            var searchResults = GetSearchResults(query);

            var trueResult = searchResults
                .FirstOrDefault(item => string.Compare(item.Text, 0, query, 0, item.Text.Length, true) == 0);
            if (trueResult == null) return null;

            var hashedLink = "https://rasp.tpu.ru" + trueResult.Url;
            var trueUrl = _urlHelper.GetFinalRedirect(hashedLink);

            var calendar = GetByLink(trueUrl, before, after);

            return calendar;
        }

        /// <summary>
        ///     Получает результат поиска на сайте rasp.tpu.ru аналогично полю ввода на главной странице.
        /// </summary>
        /// <param name="query">Запрос для поиска.</param>
        /// <returns>Перечисление результатов.</returns>
        public IEnumerable<QueryResultItemModel> GetSearchResults(string query)
        {
            var queryUrl = "https://rasp.tpu.ru/select/search/main.html?q=" + query;
            var queryResultJson = _urlHelper.GetRequestContent(queryUrl);
            var queryResult = JsonConvert.DeserializeObject<QueryResultModel>(queryResultJson);
            var result = queryResult.Result;

            return result;
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
                .Select(x => _urlHelper.GetRequestContent(x))
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