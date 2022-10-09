using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HRAshton.TimeElite.RaspTpuParser.Helpers
{
    /// <summary>
    /// Помощник для получения xor-ключей с сайта Расписания.
    /// </summary>
    internal class XorKeyFetcher
    {
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="httpClient">Http-клиент.</param>
        public XorKeyFetcher(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        private HttpClient HttpClient { get; }

        /// <summary>
        /// Получить xor-ключ.
        /// </summary>
        /// <param name="requestKey">Ключ запроса.</param>
        /// <param name="csrfToken">CSRF-токен.</param>
        /// <returns>Xor-ключ.</returns>
        public async Task<string> FetchXorKeyAsync(string requestKey, string csrfToken)
        {
            using var content = new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    { "token", "token" },
                    { "content", requestKey },
                });
            content.Headers.Add("x-csrf-token", csrfToken);

            var task = await HttpClient.PostAsync(RaspTpuUrlHelper.XorPage, content);

            var jsonResponse = await task.Content.ReadAsStringAsync();

            var receivedKey = JsonConvert.DeserializeObject<XorKeyFetchingResult>(jsonResponse);
            if (receivedKey.Message != "OK")
            {
                throw new JsonException("Не удалось получить xor-ключ.");
            }

            return receivedKey.Content;
        }

        [Serializable]
        private class XorKeyFetchingResult
        {
            public string Content { get; set; }

            public string Message { get; set; }
        }
    }
}