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
            var result = _raspTruIcalConverter.GetByQuery("8б61");
            
            CheckCommonHealth(result);
        }

        [TestMethod]
        public void GetByQueryTest_WrongGroupNameExample_NotNullGrigorianCalendarReturned()
        {
            object result = _raspTruIcalConverter.GetByQuery("0000");

            Assert.IsNull(result);
        }

        [TestMethod]
        [DeploymentItem(@"RaspTpuIcalConverterTests\Asserts\8v91_2019_17.html")]
        public void GetByHtmlTest_8d91mock_TrueCalendar()
        {
            const string path = @"Asserts\8v91_2019_17.html";
            var html = File.ReadAllText(path);
            var result = _raspTruIcalConverter.GetByHtml(html);
            
            CheckCommonHealth(result);
        }

        [TestMethod]
        [DeploymentItem(@"RaspTpuIcalConverterTests\Asserts\8b61_2019_26.html")]
        public void GetByHtmlTest_8b61mock_TrueCalendar()
        {
            const string path = @"Asserts\8b61_2019_26.html";
            var html = File.ReadAllText(path);
            var result = _raspTruIcalConverter.GetByHtml(html);

            CheckCommonHealth(result);

            Assert.IsTrue(result.Events.All(x => x.DtStart.Date.DayOfWeek != DayOfWeek.Monday));
        }

        [TestMethod]
        public void GetByHtmlTest_Rodina_ConsultationIsNotEmpty()
        {
            const string url = "https://rasp.tpu.ru/user_296870/2019/13/view.html";
            var result = _raspTruIcalConverter.GetByLink(url);

            CheckCommonHealth(result);

            Assert.IsTrue(result.Events.First().Name == "Консультация");
        }

        [TestMethod]
        public void GetByQueryTest_ReturnsEmpty()
        {
            var result = _raspTruIcalConverter.GetSearchResults("Hello There!");

            Assert.IsFalse(result.Any());
        }

        [TestMethod]
        public void GetByQueryTest_ReturnsSingleResult()
        {
            var result = _raspTruIcalConverter.GetSearchResults("8б61");

            Assert.IsTrue(result.Count() == 1);
        }

        [TestMethod]
        public void GetByQueryTest_ResultExists()
        {
            var result = _raspTruIcalConverter.GetSearchResults("это71п");

            Assert.IsTrue(result.Count(x => x.Id == 11047) == 1);
        }


        private static void CheckCommonHealth(Calendar result)
        {
            // Вернулся валидный календарь.
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(Calendar));

            // Имя календаря не содержит двух пробелов подряд.
            Assert.IsFalse(Regex.IsMatch(result.Name, @"\s\s"));
            Assert.IsFalse(result.Name.Length < 2);

            // У всех событий есть имена.
            var elementsWithEmptyNames = result.Events.Where(x => string.IsNullOrEmpty(x.Name));
            Assert.IsFalse(elementsWithEmptyNames.Any());
        }
    }
}