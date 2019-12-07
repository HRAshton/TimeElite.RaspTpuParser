using RaspTpuIcalConverter;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using Ical.Net;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable once CheckNamespace
namespace RaspTpuIcalConverter.Tests
{
    [TestClass]
    public class RaspTruIcalConverterTests
    {
        private RaspTruIcalConverter _raspTruIcalConverter;
        private IMemoryCache _memoryCache;

        [TestInitialize]
        public void Init()
        {
            var handler = new HttpClientHandler
            {
                Proxy = new WebProxy(new Uri("http://10.0.25.3:8080")) { UseDefaultCredentials = true },
                DefaultProxyCredentials = CredentialCache.DefaultCredentials
            };
            var client = new HttpClient(handler);

            _raspTruIcalConverter = new RaspTruIcalConverter(client);
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
        }

        [TestMethod]
        public void GetByQueryTest_RightGroupNameExample_NotNullGrigorianCalendarReturned()
        {
            object result = _raspTruIcalConverter.GetByQuery("8б61");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(Calendar));
        }

        [TestMethod]
        public void GetByQueryTest_WrongGroupNameExample_NotNullGrigorianCalendarReturned()
        {
            object result = _raspTruIcalConverter.GetByQuery("0000");

            Assert.IsNull(result);
        }

        [TestMethod]
        [DeploymentItem(@"RaspTpuIcalConverterTests\Asserts\8d91_2019_10.html")]
        public void GetByHtmlTest_8d91mock_TrueCalendar()
        {
            const string path = @"Asserts\8d91_2019_10.html";
            var html = File.ReadAllText(path);
            var result = _raspTruIcalConverter.GetByHtml(html);

            // Вернулся валидный календарь.
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(Calendar));

            // Имя календаря не содержит двух пробелов подряд.
            Assert.IsFalse(Regex.IsMatch(result.Name, @"\s\s"));

            // У всех событий есть имена.
            var elementsWithEmptyNames = result.Events.Where(x => string.IsNullOrEmpty(x.Name));
            Assert.IsFalse(elementsWithEmptyNames.Any());
        }

        [TestMethod]
        public void GetByHtmlTest_Rodina_ConsultationIsNotEmpty()
        {
            const string url = "https://rasp.tpu.ru/user_296870/2019/13/view.html";
            var result = _raspTruIcalConverter.GetByLink(url);

            // Вернулся валидный календарь.
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(Calendar));

            Assert.IsTrue(result.Events.First().Name == "Консультация");
        }

        [TestMethod()]
        public void GetByQueryTest_ReturnsEmpty()
        {
            var result = _raspTruIcalConverter.GetSearchResults("Hello There!");

            Assert.IsFalse(result.Any());
        }

        [TestMethod()]
        public void GetByQueryTest_ReturnsSingleResult()
        {
            var result = _raspTruIcalConverter.GetSearchResults("8б61");

            Assert.IsTrue(result.Count() == 1);
        }

        [TestMethod()]
        public void GetByQueryTest_ResultExists()
        {
            var result = _raspTruIcalConverter.GetSearchResults("105");

            Assert.IsTrue(result.Count(x => x.Id == 11840) == 1);
        }
    }
}