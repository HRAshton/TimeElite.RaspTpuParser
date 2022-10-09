using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace HRAshton.TimeElite.RaspTpuParser.Extensions
{
    /// <summary>
    /// Расширения Http-клиента.
    /// </summary>
    public static class HttpClientExtensions
    {
        /// <summary>
        /// Получает ссылку после переадресации.
        /// </summary>
        /// <param name="httpClient">Клиент http.</param>
        /// <param name="url">Входной url.</param>
        /// <returns>Ссылка после переадресации.</returns>
        public static async Task<string> GetFinalRedirectAsync(this HttpClient httpClient, string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return url;
            }

            try
            {
                var task = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

                return task.RequestMessage.RequestUri.AbsoluteUri;
            }
            catch (WebException)
            {
                return url;
            }
        }
    }
}