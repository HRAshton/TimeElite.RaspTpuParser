using System.Net;
using System.Net.Http;

namespace HRAshton.TimeElite.RaspTpuParser.Extensions
{
    /// <summary>
    /// Расширения Http-клиента.
    /// </summary>
    public static class HttpClientExtensions
    {
        /// <summary>
        /// Получает содержимое по Url и возвращает его в виде строки.
        /// </summary>
        /// <param name="httpClient">Клиент http.</param>
        /// <param name="url">Адрес для получения содержимого.</param>
        /// <returns>Содержимое.</returns>
        public static string GetRequestContent(this HttpClient httpClient, string url)
        {
            var response = httpClient.GetStringAsync(url);

            response.Wait();
            var result = response.Result;

            return result;
        }

        /// <summary>
        /// Получает ссылку после переадресации.
        /// </summary>
        /// <param name="httpClient">Клиент http.</param>
        /// <param name="url">Входной url.</param>
        /// <returns>Ссылка после переадресации.</returns>
        public static string GetFinalRedirect(this HttpClient httpClient, string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return url;
            }

            try
            {
                var task = httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                task.Wait();

                return task.Result.RequestMessage.RequestUri.AbsoluteUri;
            }
            catch (WebException)
            {
                return url;
            }
        }
    }
}