using System;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace HRAshton.TimeElite.RaspTpuParser.Helpers
{
    /// <summary>
    /// Дешифратор зашифрованных XOR-ом тегов (с названиями пар).
    /// </summary>
    internal class RaspTpuDecryptor
    {
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="xorKeyFetcher">Помощник для получения xor-ключей с сайта Расписания.</param>
        public RaspTpuDecryptor(XorKeyFetcher xorKeyFetcher)
        {
            XorKeyFetcher = xorKeyFetcher;
        }

        private XorKeyFetcher XorKeyFetcher { get; }

        /// <summary>
        /// Подменить InnerText зашифрованных тегов.
        /// </summary>
        /// <param name="html">HTML-документ.</param>
        /// <param name="key">Xor-ключ.</param>
        public void DecryptAll(HtmlDocument html, byte[] key = null)
        {
            key ??= ParseKey(html);

            var nodesWithEncryptedInnerText = html.DocumentNode.SelectNodes("//*[@data-encrypt]");
            foreach (var htmlNode in nodesWithEncryptedInnerText)
            {
                DecodeInnerTexts(htmlNode, key);
                DecodeTitles(htmlNode, key);
            }
        }

        private byte[] ParseKey(HtmlDocument html)
        {
            string requestKey = GetMetaTagContent(html, "encrypt");
            string csrfToken = GetMetaTagContent(html, "csrf-token");

            var receivedKey = XorKeyFetcher.FetchXorKey(requestKey, csrfToken);

            var keyBytes = receivedKey
                .ToCharArray()
                .Select(chr => (byte)chr)
                .ToArray();

            return keyBytes;
        }

        private static string GetMetaTagContent(HtmlDocument html, string name)
        {
            return html.DocumentNode
                .SelectSingleNode($"//meta[@name='{name}']")
                .Attributes["content"]
                .Value;
        }

        private void DecodeInnerTexts(HtmlNode htmlNode, byte[] key)
        {
            var ciphertext = htmlNode.Attributes["data-encrypt"].Value;
            var plainText = Decrypt(ciphertext, key);

            htmlNode.ChildNodes.Clear();
            htmlNode.ChildNodes.Add(HtmlNode.CreateNode(plainText));
        }

        private void DecodeTitles(HtmlNode htmlNode, byte[] key)
        {
            var encodedTitleAttr = htmlNode.Attributes.FirstOrDefault(attr => attr.Name == "data-title");
            if (encodedTitleAttr == null)
            {
                return;
            }

            var encodedTitle = encodedTitleAttr.Value;
            var plainTitle = Decrypt(encodedTitle, key);

            htmlNode.SetAttributeValue("title", plainTitle);
        }

        /// <summary>
        /// Расшифровать текст.
        /// </summary>
        /// <param name="base64Text">Зашифрованный текст.</param>
        /// <param name="key">Байты ключа.</param>
        /// <returns>Расшифрованный текст.</returns>
        private string Decrypt(string base64Text, byte[] key)
        {
            var encodedBytes = Convert.FromBase64String(base64Text);
            var encodedChars = Encoding.UTF8.GetString(encodedBytes).ToCharArray();

            var decryptedBytes = encodedChars
                .Select((chr, index) => (ushort)(chr ^ key[index % key.Length]))
                .Select(t => char.ConvertFromUtf32(t));

            var planeText = string.Join(string.Empty, decryptedBytes);

            return planeText;
        }
    }
}