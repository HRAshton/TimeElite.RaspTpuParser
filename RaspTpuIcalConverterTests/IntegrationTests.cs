using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using HRAshton.TimeElite.RaspTpuParser.Helpers;
using HRAshton.TimeElite.RaspTpuParser.Models;
using HRAshton.TimeElite.RaspTpuParser.Parsers;
using Ical.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HRAshton.TimeElite.RaspTpuParser.Tests
{
    /// <summary>
    /// Интеграционные тесты библиотеки.
    /// </summary>
    [TestClass]
    public class IntegrationTests
    {
        private RaspTpuIcalConverter raspTpuIcalConverter;

        /// <summary>
        /// Инициализация тестов.
        /// </summary>
        [TestInitialize]
        public void Init()
        {
            var client = new HttpClient();
            var pageParser = new PageParser();
            var raspTpuDecryptor = new RaspTpuDecryptor(new XorKeyFetcher(client));

            raspTpuIcalConverter = new RaspTpuIcalConverter(client, pageParser, raspTpuDecryptor);
        }

        /// <summary>
        /// Тест получения расписания по названию группы. Успешный.
        /// </summary>
        [TestMethod]
        public void GetByQueryTest_RightGroupNameExample_NotNullGregorianCalendarReturned()
        {
            var result = raspTpuIcalConverter.GetByQuery("8Т01");

            CheckCommonHealth(result);
        }

        /// <summary>
        /// Тест получения расписания по названию группы. Провальный.
        /// </summary>
        [TestMethod]
        public void GetByQueryTest_WrongGroupNameExample_NotNullGregorianCalendarReturned()
        {
            object result = raspTpuIcalConverter.GetByQuery("0000");

            Assert.IsNull(result);
        }

        /// <summary>
        /// Тест получения расписания группы по ссылке. Успешный.
        /// </summary>
        [TestMethod]
        public void GetByHtmlTest_8k24WithHolidays_TrueCalendar()
        {
            const string url = "https://rasp.tpu.ru/gruppa_39211/2022/1/view.html";

            var result = raspTpuIcalConverter.GetByLink(url);

            var threeLastDaysOnly = result.Events.All(lsn => lsn.DtStart.Day >= 1 && lsn.DtStart.Day <= 3);

            CheckCommonHealth(result);
            Assert.AreEqual("8К24", result.Name);
            Assert.IsTrue(threeLastDaysOnly);
        }

        /// <summary>
        /// Тест получения расписания преподавателя по ссылке. Успешный.
        /// </summary>
        [TestMethod]
        public void GetByHtmlTest_Rodina_ConsultationIsNotEmpty()
        {
            const string url = "https://rasp.tpu.ru/user_296870/2020/4/view.html";
            var result = raspTpuIcalConverter.GetByLink(url);

            CheckCommonHealth(result);

            Assert.IsTrue(result.Events.First().Name == "Практ.психология");
        }

        /// <summary>
        /// Тест получения результатов поиска. Без результатов.
        /// </summary>
        [TestMethod]
        public void GetByQueryTest_ReturnsEmpty()
        {
            var result = raspTpuIcalConverter.GetSearchResults("Hello There!");

            Assert.IsFalse(result.Any());
        }

        /// <summary>
        /// Тест получения результатов поиска по полной подстроке. Успешный.
        /// Учитывается только количество результатов.
        /// </summary>
        [TestMethod]
        public void GetByQueryTest_ReturnsSingleResult()
        {
            var result = raspTpuIcalConverter.GetSearchResults("8т01");

            Assert.AreEqual(1, result.Length);
        }

        /// <summary>
        /// Тест получения результатов поиска по полной подстроке. Успешный.
        /// Учитывается идентификатор группы.
        /// </summary>
        [TestMethod]
        public void GetByQueryTest_ResultExists()
        {
            var result = raspTpuIcalConverter.GetSearchResults("это20ф");

            Assert.AreEqual(1, result.Count(item => item.Id == 18482));
        }

        private static void CheckCommonHealth(CalendarWithTimesModel result)
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

            // Времена начал пар идут в порядке возрастания.
            for (var index = 1; index < result.LessonsTimes.Count; index++)
            {
                var prev = result.LessonsTimes[index - 1];
                var curr = result.LessonsTimes[index];
                if (prev.Minutes + (prev.Hours * 60) >= curr.Minutes + (curr.Hours * 60))
                {
                    Assert.Fail();
                }

                Assert.AreEqual(curr.Hours - prev.Hours, 2);
            }
        }
    }
}