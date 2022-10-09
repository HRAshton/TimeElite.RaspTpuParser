﻿using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HRAshton.TimeElite.RaspTpuParser.Helpers;
using HtmlAgilityPack;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HRAshton.TimeElite.RaspTpuParser.Tests.Helpers
{
    [TestClass]
    public class RaspTpuDecryptorTests
    {
        private RaspTpuDecryptor decryptor;

        [TestInitialize]
        public void Init()
        {
            decryptor = new RaspTpuDecryptor(new XorKeyFetcher(new HttpClient()));
        }

        [TestMethod]
        [DeploymentItem(@"RaspTpuIcalConverterTests\Asserts\8k24_2022_1.html")]
        public async Task DecryptAllTest()
        {
            const string path = @"Asserts\8k24_2022_1.html";
            var html = await File.ReadAllTextAsync(path);
            var key = Encoding.UTF8.GetBytes("hVgilyimNiWSxRkx");

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            await decryptor.DecryptAllAsync(doc, key);
            var nodes = doc.DocumentNode.SelectNodes("//*[@data-encrypt]");

            Assert.IsNotNull(nodes);
            Assert.IsTrue(nodes.Count >= 2);
            Assert.IsTrue(nodes[0].InnerText == "8К24");
            Assert.IsTrue(nodes[1].InnerText == "Соц.осн.инж. проект.");
            Assert.IsTrue(nodes[1].Attributes["title"].Value == "Социальные основы инженерного проектирования");
        }
    }
}