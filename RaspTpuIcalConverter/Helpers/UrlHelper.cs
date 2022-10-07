using System;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Caching.Memory;

namespace HRAshton.TimeElite.RaspTpuParser.Helpers
{
    /// <summary>
    /// Набор функций для работы с Url и сетью.
    /// </summary>
    internal class UrlHelper
    {
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="httpClient">Клиент для запросов (с прокси-сервером, если надо).</param>
        /// <param name="memoryCache">Кэш.</param>
        public UrlHelper(HttpClient httpClient, IMemoryCache memoryCache)
        {
            HttpClient = httpClient;
            MemoryCache = memoryCache;
        }

        private HttpClient HttpClient { get; }

        private IMemoryCache MemoryCache { get; }

        /// <summary>
        /// Проверяет, является ли строка абсолютным Url.
        /// </summary>
        /// <param name="url">Строка, содержащая url.</param>
        /// <returns>Является ли строка абсолютным Url.</returns>
        public bool IsAbsoluteUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }

        /// <summary>
        /// Получает содержимое по Url и возвращает его в виде строки.
        /// </summary>
        /// <param name="url">Адрес для получения содержимого.</param>
        /// <param name="cacheTime">Время действительности кэша.</param>
        /// <returns>Содержимое.</returns>
        public string GetRequestContent(string url, TimeSpan cacheTime)
        {
            var response = MemoryCache.GetOrCreate(
                $"{url}",
                entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = cacheTime;
                    var res = HttpClient.GetStringAsync(url);

                    return res;
                });

            var result = response.Result;

            return result;
        }

        /// <summary>
        /// Получает ссылку после переадресации.
        /// </summary>
        /// <param name="url">Входной url.</param>
        /// <returns>Ссылка после переадресации.</returns>
        public string GetFinalRedirect(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return url;
            }

            try
            {
                var resp = HttpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).Result;

                return resp.RequestMessage.RequestUri.AbsoluteUri;
            }
            catch (WebException)
            {
                // Return the last known good URL
                return url;
            }
        }
    }
}