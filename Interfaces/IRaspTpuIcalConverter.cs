using HRAshton.TimeElite.RaspTpuParser.Models;

namespace HRAshton.TimeElite.RaspTpuParser
{
    /// <summary>
    /// Конвертер расписания в ICal.
    /// </summary>
    public interface IRaspTpuIcalConverter
    {
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
        CalendarWithTimesModel GetByHtml(string pageHtml);

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
        CalendarWithTimesModel GetByLink(string link);

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
        CalendarWithTimesModel GetByLink(
            string link,
            byte before,
            byte skipBetweenCurrentWeekAndAfter,
            byte after);

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
        CalendarWithTimesModel GetByHash(string hash);

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
        CalendarWithTimesModel GetByHash(
            string hash,
            byte before,
            byte skipBetweenCurrentWeekAndAfter,
            byte after);

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
        CalendarWithTimesModel GetByQuery(string query);

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
        CalendarWithTimesModel GetByQuery(
            string query,
            byte before,
            byte skipBetweenCurrentWeekAndAfter,
            byte after);

        /// <summary>
        /// Получает результат поиска на сайте rasp.tpu.ru аналогично полю ввода на главной странице.
        /// </summary>
        /// <param name="query">Запрос для поиска.</param>
        /// <returns>Перечисление результатов.</returns>
        SearchResultItemModel[] GetSearchResults(string query);
    }
}