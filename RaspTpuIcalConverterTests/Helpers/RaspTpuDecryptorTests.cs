using Microsoft.VisualStudio.TestTools.UnitTesting;
using RaspTpuIcalConverter.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using HtmlAgilityPack;

namespace RaspTpuIcalConverter.Helpers.Tests
{
    [TestClass()]
    public class RaspTpuDecryptorTests
    {
        private RaspTpuDecryptor decryptor;

        [TestInitialize]
        public void Init()
        {
            decryptor = new RaspTpuDecryptor();
            decryptor.SetKey("RG82tMS3FrNvOPCZ");
        }

        [TestMethod()]
        public void DecryptTest()
        {
            var text = decryptor.Decrypt("0b/RpdCmBUXRkg==");

            Assert.IsTrue(text == "ЭТО71П");
        }

        [TestMethod]
        [DeploymentItem(@"RaspTpuIcalConverterTests\Asserts\8v91_2019_17.html")]
        public void DecryptAllTest()
        {
            const string path = @"Asserts\8v91_2019_17.html";
            var html = File.ReadAllText(path);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            decryptor.DecryptAll(ref doc);

            var nodes = doc.DocumentNode.SelectNodes("//*[@data-encrypt]");

            Assert.IsTrue(nodes?[0]?.InnerText == "8В91");
            Assert.IsTrue(nodes?[1]?.InnerText == "Математика 1");
            Assert.IsTrue(nodes?[1]?.Attributes["title"].Value == "Математика 1");
        }
    }
}