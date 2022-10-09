using System;
using System.Collections.Generic;
using System.Net.Http;
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
        public string FetchXorKey(string requestKey, string csrfToken)
        {
            var content = new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    { "token", "token" },
                    { "content", requestKey },
                });
            content.Headers.Add("x-csrf-token", csrfToken);

            var task = HttpClient.PostAsync(RaspTpuUrlHelper.XorPage, content);
            task.Wait();

            var jsonResponse = task.Result.Content.ReadAsStringAsync();
            jsonResponse.Wait();

            var receivedKey = JsonConvert.DeserializeObject<XorKeyFetchingResult>(jsonResponse.Result);
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