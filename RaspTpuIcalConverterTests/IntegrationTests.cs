using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HRAshton.TimeElite.RaspTpuParser.Helpers;
using HRAshton.TimeElite.RaspTpuParser.Models;
using HRAshton.TimeElite.RaspTpuParser.Parsers;
using Ical.Net;
using NUnit.Framework;

namespace HRAshton.TimeElite.RaspTpuParser.Tests
{
    /// <summary>
    /// Интеграционные тесты библиотеки.
    /// </summary>
    public class IntegrationTests
    {
        private RaspTpuIcalConverter raspTpuIcalConverter;

        /// <summary>
        /// Инициализация тестов.
        /// </summary>
        [SetUp]
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
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        [Test]
        public async Task GetByQueryTest_RightGroupNameExample_NotNullGregorianCalendarReturned()
        {
            var result = await raspTpuIcalConverter.GetByQueryAsync("8Т01");

            CheckCommonHealth(result);
        }

        /// <summary>
        /// Тест получения расписания по названию группы. Провальный.
        /// </summary>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        [Test]
        public async Task GetByQueryTest_WrongGroupNameExample_NotNullGregorianCalendarReturned()
        {
            object result = await raspTpuIcalConverter.GetByQueryAsync("0000");

            Assert.IsNull(result);
        }

        /// <summary>
        /// Тест получения расписания группы по ссылке. Успешный.
        /// </summary>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        [Test]
        public async Task GetByHtmlTest_8k24WithHolidays_TrueCalendar()
        {
            const string url = "https://rasp.tpu.ru/gruppa_39211/2022/1/view.html";

            var result = await raspTpuIcalConverter.GetByLinkAsync(url);

            var threeLastDaysOnly = result.Events.All(lsn => lsn.DtStart.Day is >= 1 and <= 3);

            var testingLessons = result.Events
                .Where(evt => evt.Name == "Входное независимое тестирование")
                .ToArray();
            var twoSameTestingLessons = testingLessons.Length == 2
                                        && testingLessons[0].DtStart.Date.DayOfWeek ==
                                        testingLessons[1].DtStart.Date.DayOfWeek
                                        && testingLessons[0].Description ==
                                        testingLessons[1].Description;

            CheckCommonHealth(result);
            Assert.AreEqual("8К24", result.Name);
            Assert.IsTrue(threeLastDaysOnly);
            Assert.IsTrue(twoSameTestingLessons);
        }

        /// <summary>
        /// Тест получения расписания преподавателя по ссылке. Успешный.
        /// </summary>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        [Test]
        public async Task GetByHtmlTest_Rodina_ConsultationIsNotEmpty()
        {
            const string url = "https://rasp.tpu.ru/user_296870/2020/4/view.html";
            var result = await raspTpuIcalConverter.GetByLinkAsync(url);

            CheckCommonHealth(result);

            Assert.IsTrue(result.Events.First().Name == "Практ.психология");
        }

        /// <summary>
        /// Тест получения результатов поиска. Без результатов.
        /// </summary>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        [Test]
        public async Task GetByQueryTest_ReturnsEmpty()
        {
            var result = await raspTpuIcalConverter.GetSearchResultsAsync("Hello There!");

            Assert.IsFalse(result.Any());
        }

        /// <summary>
        /// Тест получения результатов поиска по полной подстроке. Успешный.
        /// Учитывается только количество результатов.
        /// </summary>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        [Test]
        public async Task GetByQueryTest_ReturnsSingleResult()
        {
            var result = await raspTpuIcalConverter.GetSearchResultsAsync("8т01");

            Assert.AreEqual(1, result.Length);
        }

        /// <summary>
        /// Тест получения результатов поиска по полной подстроке. Успешный.
        /// Учитывается идентификатор группы.
        /// </summary>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        [Test]
        public async Task GetByQueryTest_ResultExists()
        {
            var result = await raspTpuIcalConverter.GetSearchResultsAsync("это20ф");

            Assert.AreEqual(1, result.Count(item => item.Id == 18482));
        }

        private static void CheckCommonHealth(CalendarWithTimesModel result)
        {
            // Вернулся валидный календарь.
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<Calendar>(result);

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