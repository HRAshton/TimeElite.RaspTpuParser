using System;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace RaspTpuIcalConverter.Helpers
{
    /// <summary>
    /// Дешифратор зашифрованных XOR-ом тегов (с названиями пар).
    /// </summary>
    public class RaspTpuDecryptor
    {
        private byte[] key;

        /// <summary>
        /// Установить ключ.
        /// </summary>
        /// <param name="keyValue">Ключ в строковом формате.</param>
        public void SetKey(string keyValue)
        {
            key = keyValue
                .ToCharArray()
                .Select(x => (byte)x)
                .ToArray();
        }

        /// <summary>
        /// Подменить InnerText зашифрованных тегов.
        /// </summary>
        /// <param name="html">HTML-документ.</param>
        public void DecryptAll(ref HtmlDocument html)
        {
            ParseAndStoreKey(html);

            var nodesWithEncryptedInnerText = html.DocumentNode.SelectNodes("//*[@data-encrypt]");
            foreach (var htmlNode in nodesWithEncryptedInnerText)
            {
                DecodeInnerTexts(htmlNode);
                DecodeTitles(htmlNode);
            }
        }

        /// <summary>
        /// Расшифровать текст.
        /// </summary>
        /// <param name="base64Text">Зашифрованный текст.</param>
        /// <returns>Расшифрованный текст.</returns>
        public string Decrypt(string base64Text)
        {
            var encodedBytes = Convert.FromBase64String(base64Text);
            var encodedChars = Encoding.UTF8.GetString(encodedBytes).ToCharArray();

            var decryptedBytes = encodedChars
                .Select((chr, index) => (ushort)(chr ^ key[index % key.Length]))
                .Select(t => char.ConvertFromUtf32(t));

            var planeText = string.Join(string.Empty, decryptedBytes);

            return planeText;
        }

        private void ParseAndStoreKey(HtmlDocument html)
        {
            var keyValue = html.DocumentNode
                .SelectSingleNode("//meta[@name='encrypt']")
                .Attributes["content"]
                .Value;

            SetKey(keyValue);
        }

        private void DecodeInnerTexts(HtmlNode htmlNode)
        {
            var ciphertext = htmlNode.Attributes["data-encrypt"].Value;
            var plainText = Decrypt(ciphertext);

            htmlNode.ChildNodes.Clear();
            htmlNode.ChildNodes.Add(HtmlNode.CreateNode(plainText));
        }

        private void DecodeTitles(HtmlNode htmlNode)
        {
            var encodedTitleAttr = htmlNode.Attributes.FirstOrDefault(attr => attr.Name == "data-title");
            if (encodedTitleAttr == null)
            {
                return;
            }

            var encodedTitle = encodedTitleAttr.Value;
            var plainTitle = Decrypt(encodedTitle);

            htmlNode.SetAttributeValue("title", plainTitle);
        }
    }
}
