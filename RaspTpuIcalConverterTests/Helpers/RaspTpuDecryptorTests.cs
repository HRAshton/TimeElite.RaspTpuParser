using System.IO;
using HtmlAgilityPack;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RaspTpuIcalConverter.Helpers;

namespace RaspTpuIcalConverter.Tests.Helpers
{
    [TestClass]
    public class RaspTpuDecryptorTests
    {
        private RaspTpuDecryptor decryptor;

        [TestInitialize]
        public void Init()
        {
            decryptor = new RaspTpuDecryptor();
            decryptor.SetKey("BhAXvvd1pJd6CZJS");
        }

        [TestMethod]
        public void DecryptTest()
        {
            var text = decryptor.Decrypt("0a/RitGfakbRkg==");

            Assert.IsTrue(text == "ЭТО20Ф");
        }

        [TestMethod]
        [DeploymentItem(@"RaspTpuIcalConverterTests\Asserts\8t01_2020_10.html")]
        public void DecryptAllTest()
        {
            const string path = @"Asserts\8t01_2020_10.html";
            var html = File.ReadAllText(path);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            decryptor.DecryptAll(ref doc);

            var nodes = doc.DocumentNode.SelectNodes("//*[@data-encrypt]");

            Assert.IsNotNull(nodes);
            Assert.IsTrue(nodes.Count >= 2);
            Assert.IsTrue(nodes[0].InnerText == "8Т01");
            Assert.IsTrue(nodes[1].InnerText == "Химия 1");
            Assert.IsTrue(nodes[1].Attributes["title"].Value == "Химия 1");
        }
    }
}