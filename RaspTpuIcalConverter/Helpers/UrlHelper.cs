using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace RaspTpuIcalConverter.Helpers
{
    /// <summary>
    ///     Набор функций для работы с Url и сетью.
    /// </summary>
    internal class UrlHelper
    {
        /// <summary>
        ///     Конструктор.
        /// </summary>
        /// <param name="httpClient">Клиент для запросов (с прокси, если надо).</param>
        public UrlHelper(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        private HttpClient HttpClient { get; }

        /// <summary>
        ///     Проверяет, является ли строка абсолютным Url.
        /// </summary>
        /// <param name="url">Строка, содержащая url.</param>
        /// <returns>Является ли строка абсолютным Url.</returns>
        public bool IsAbsoluteUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }

        /// <summary>
        ///     Получает содержимое по Url и возвращает его в виде строки.
        /// </summary>
        /// <param name="url">Адрес для получения контента.</param>
        /// <returns>Контент.</returns>
        public string GetRequestContent(string url)
        {
            var response = HttpClient.GetStringAsync(url);

            var result = response.Result;

            return result;
        }
    }
}