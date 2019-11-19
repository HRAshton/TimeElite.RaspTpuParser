using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using Ical.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable once CheckNamespace
namespace RaspTpuIcalConverter.Tests
{
    [TestClass]
    public class RaspTruIcalConverterTests
    {
        private RaspTruIcalConverter _raspTruIcalConverter;

        [TestInitialize]
        public void Init()
        {
            var handler = new HttpClientHandler
            {
                Proxy = new WebProxy(new Uri("http://10.0.25.3:8080")) {UseDefaultCredentials = true},
                DefaultProxyCredentials = CredentialCache.DefaultCredentials
            };
            var client = new HttpClient(handler);

            _raspTruIcalConverter = new RaspTruIcalConverter(client);
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
    }
}