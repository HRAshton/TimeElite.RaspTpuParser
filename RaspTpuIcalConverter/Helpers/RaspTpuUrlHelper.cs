namespace HRAshton.TimeElite.RaspTpuParser.Helpers
{
    /// <summary>
    /// Помощник для получения адресов страниц сайта Расписания.
    /// </summary>
    internal static class RaspTpuUrlHelper
    {
        /// <summary>
        /// Адрес страницы для получения XOR-расписания.
        /// </summary>
        public const string XorPage = "https://rasp.tpu.ru/data/encrypt/decrypt.html";

        /// <summary>
        /// Создать адрес по относительному пути.
        /// </summary>
        /// <param name="relationPath">Относительный путь.</param>
        /// <returns>Адрес страницы на сайте Расписания.</returns>
        public static string CreateFromRelationPath(string relationPath)
        {
            return "https://rasp.tpu.ru" + relationPath;
        }

        /// <summary>
        /// Создать адрес по hash.
        /// </summary>
        /// <param name="hash">Hash расписания.</param>
        /// <returns>Адрес страницы на сайте Расписания.</returns>
        public static string CreateFromHash(string hash)
        {
            return "https://rasp.tpu.ru/redirect/kalendar.html?hash=" + hash;
        }

        /// <summary>
        /// Создать адрес для поиска страницы Расписания.
        /// </summary>
        /// <param name="query">Поисковый запрос.</param>
        /// <returns>Адрес страницы на сайте Расписания.</returns>
        public static string CreateQuery(string query)
        {
            return "https://rasp.tpu.ru/select/search/main.html?q=" + query;
        }

        /// <summary>
        /// Получить адрес страницы с расписанием.
        /// </summary>
        /// <param name="id">Идентификатор расписания.</param>
        /// <param name="year">Год начала обучения.</param>
        /// <param name="week">Номер недели.</param>
        /// <param name="delta">Смещение номера недели.</param>
        /// <returns>Адрес страницы с расписанием.</returns>
        public static string CreateForWeek(string id, ushort year, byte week, int delta)
        {
            return $"https://rasp.tpu.ru/{id}/{year}/{week + delta}/view.html";
        }
    }
}