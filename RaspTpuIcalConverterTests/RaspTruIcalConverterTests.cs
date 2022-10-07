using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using HRAshton.TimeElite.RaspTpuParser.RaspTpuModels;
using Ical.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HRAshton.TimeElite.RaspTpuParser.Tests
{
    /// <summary>
    /// Тесты модуля парсинга расписания.
    /// </summary>
    [TestClass]
    public class RaspTruIcalConverterTests
    {
        private RaspTpuIcalConverter raspTpuIcalConverter;

        /// <summary>
        /// Инициализация тестов.
        /// </summary>
        [TestInitialize]
        public void Init()
        {
            var client = new HttpClient();

            raspTpuIcalConverter = new RaspTpuIcalConverter(client);
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
        /// Тест получения расписания по содержимому html-страницы. Успешный.
        /// </summary>
        [TestMethod]
        [DeploymentItem(@"RaspTpuIcalConverterTests\Asserts\8t01_2020_4.html")]
        public void GetByHtmlTest_8t01mock_TrueCalendar()
        {
            const string path = @"Asserts\8t01_2020_4.html";
            var html = File.ReadAllText(path);

            var result = raspTpuIcalConverter.GetByHtml(html);

            CheckCommonHealth(result);
        }

        /// <summary>
        /// Тест получения расписания с выходными днями по содержимому html-страницы. Успешный.
        /// </summary>
        [TestMethod]
        [DeploymentItem(@"RaspTpuIcalConverterTests\Asserts\8t01_2020_10.html")]
        public void GetByHtmlTest_8b61mockWithHoliday_TrueCalendar()
        {
            const string path = @"Asserts\8t01_2020_10.html";
            var html = File.ReadAllText(path);
            var result = raspTpuIcalConverter.GetByHtml(html);

            CheckCommonHealth(result);

            Assert.IsTrue(result.Events.All(x => x.DtStart.Date.DayOfWeek != DayOfWeek.Wednesday));
        }

        /// <summary>
        /// Тест получения расписания по ссылке. Успешный.
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

            Assert.IsTrue(result.Count() == 1);
        }

        /// <summary>
        /// Тест получения результатов поиска по полной подстроке. Успешный.
        /// Учитывается идентификатор группы.
        /// </summary>
        [TestMethod]
        public void GetByQueryTest_ResultExists()
        {
            var result = raspTpuIcalConverter.GetSearchResults("это20ф");

            Assert.IsTrue(result.Count(x => x.Id == 18482) == 1);
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