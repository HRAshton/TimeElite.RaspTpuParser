using System;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace RaspTpuIcalConverter.Helpers
{
    /// <summary>
    /// Дешифратор запшифрованныхъ XOR-ом тегов (с названиями пар).
    /// </summary>
    public class RaspTpuDecryptor
    {
        private byte[] key;

        /// <summary>
        /// Установить ключ.
        /// </summary>
        /// <param name="key"></param>
        public void SetKey(string key)
        {
            this.key = key.ToCharArray().Select(x => (byte)x).ToArray();
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
                DecodeInnetTexts(htmlNode);
                DecodeTitles(htmlNode);
            }
        }

        /// <summary>
        /// Расшифровать текст.
        /// </summary>
        public string Decrypt(string base64text)
        {
            var encodedBytes = Convert.FromBase64String(base64text);
            var encodedChars = Encoding.UTF8.GetString(encodedBytes).ToCharArray();

            var decryptedBytes = encodedChars
                .Select((chr, index) => (ushort)(chr ^ key[index % key.Length]))
                .Select(t => char.ConvertFromUtf32(t));
            var planeText = string.Join(string.Empty, decryptedBytes);

            return planeText;
        }


        private void ParseAndStoreKey(HtmlDocument html)
        {
            var key = html.DocumentNode.SelectSingleNode("//meta[@name='encrypt']")
                .Attributes["content"]
                .Value;
            this.SetKey(key);
        }

        private void DecodeInnetTexts(HtmlNode htmlNode)
        {
            var ciphertext = htmlNode.Attributes["data-encrypt"].Value;
            var plainText = Decrypt(ciphertext);

            htmlNode.ChildNodes.Clear();
            htmlNode.ChildNodes.Add(HtmlNode.CreateNode(plainText));
        }

        private void DecodeTitles(HtmlNode htmlNode)
        {
            var encodedTitleAttr = htmlNode.Attributes.FirstOrDefault(attr => attr.Name == "data-title");
            if (encodedTitleAttr == null) return;
            var encodedTitle = encodedTitleAttr.Value;
            var plainTitle = Decrypt(encodedTitle);
            htmlNode.SetAttributeValue("title", plainTitle);
        }
    }
}
