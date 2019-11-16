using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using RaspTpuIcalConverter.Extensions;
using Calendar = Ical.Net.Calendar;

namespace RaspTpuIcalConverter.Parsers
{
    /// <summary>
    /// Парсер для html-страницы сайта rasp.tpu.ru.
    /// </summary>
    internal class PageParser
    {
        private readonly List<string> StringTimeAssociation = new List<string>(new[]
        {
            "8:30", "10:25", "12:20", "14:15", "16:10", "18:05", "20:00"
        });

        /// <summary>
        /// Преобразует строку с html-кодом страницы в объект типа <see cref="Calendar"/> (Ical.Net).
        /// </summary>
        /// У календаря есть поле Events, содержащее события. У события заполняются только атрибуты:
        /// <list type="bullet">
        /// <item> Name </item>
        /// <item> Contacts </item>
        /// <item> Location </item>
        /// <item> Description </item>
        /// <item> DtStart </item>
        /// <item> DtEnd </item>
        /// <item> Duration </item>
        /// </list>
        /// <param name="html">Строка с html-кодом страницы.</param>
        /// <returns>Календарь с названием и событиями.</returns>
        public Calendar ParsePage(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var tableNode = doc.GetElementbyId("raspisanie-table")
                .ChildNodes.FirstOrDefault(node => node.NodeType == HtmlNodeType.Element);
            var monday = GetMonday(tableNode);

            var name = doc.DocumentNode
                .SelectSingleNode("//h5[@class=\"panel-title\"]").InnerText
                .Trim();
            name = Regex.Replace(name, @"\s+", @" ");

            var tbody = tableNode // table
                ?.ChildNodes
                .LastOrDefault(node => node.NodeType == HtmlNodeType.Element); // tbody
            var calendar = GetCalendar(monday, tbody, name);

            return calendar;
        }

        private static DateTime GetMonday(HtmlNode tableNode)
        {
            var thead = tableNode
                ?.ChildNodes
                .FirstOrDefault(node => node.NodeType == HtmlNodeType.Element);

            var stringDate = thead.GetChildElementsList()
                .First()
                .GetChildElementsList()[1]
                .ChildNodes[0]
                .InnerText
                .Trim();

            var date = DateTime.Parse(stringDate, new CultureInfo("ru-RU"));

            return date;
        }

        private Calendar GetCalendar(DateTime mondayDate, HtmlNode tbody, string calendarName)
        {
            var rows = tbody.GetChildElementsList();

            var holydays = GetHolydays(rows);
            var events = ParseRow(rows.First(), holydays, mondayDate, 0);
            events.AddRange(rows.Skip(1)
                .SelectMany((row, rowIndex) => ParseRow(row, holydays, mondayDate, rowIndex + 1)));

            var calendar = new Calendar
            {
                Name = calendarName
            };
            calendar.Events.AddRange(events);

            return calendar;
        }

        private List<CalendarEvent> ParseRow(HtmlNode row, ICollection<DayOfWeek> holydays, DateTime mondayDate, int rowIndex)
        {
            var cells = row.GetChildElementsList();

            var events = new List<CalendarEvent>();

            var currentCellIndex = 0;
            for (var dayOfWeek = 0; dayOfWeek < 6; dayOfWeek++)
            {
                var currentDayOfWeek = (DayOfWeek) (dayOfWeek + 1);
                if (holydays.Contains(currentDayOfWeek))
                {
                    if (rowIndex == 0) currentCellIndex++;
                    continue;
                }

                currentCellIndex++;
                var dateTime = GetStartDateTime(row, mondayDate, dayOfWeek);
                var cellEvents = ParseCell(cells[currentCellIndex], dateTime);

                events.AddRange(cellEvents);
            }

            return events;
        }

        private DateTime GetStartDateTime(HtmlNode row, DateTime mondayDate, int dayOfWeek)
        {
            var firstLessonStart = mondayDate
                .AddDays(dayOfWeek)
                .AddMinutes(510); // 8:30

            var lessonIndex = StringTimeAssociation
                .FindIndex(x => row.GetChildElementsList().First().InnerText.Contains(x));

            var lessonStart = firstLessonStart.AddMinutes(lessonIndex * (20 + 95));

            return lessonStart;
        }

        private static IEnumerable<CalendarEvent> ParseCell(HtmlNode td, DateTime dateTime)
        {
            var isEmptyCell = !td.ChildNodes.Any();
            if (isEmptyCell) return new List<CalendarEvent>();

            var hrGroups = td.ChildNodes
                .Where(x => x.Name != "#text")
                .ToList()
                .SplitBy(x => x.Name == "hr");

            var events = new List<CalendarEvent>();
            for (var i = 0; i < hrGroups.Count; i++)
            {
                var group = hrGroups[i];

                HtmlNode spanWithName = null;
                for (var j = i; j >= 0; j--)
                {
                    spanWithName = hrGroups[j].SelectMany(x => x.ChildNodes).FirstOrDefault(x => x.Name == "span");
                    if (spanWithName != null) break;
                }

                HtmlNode bWithType = null;
                for (var j = i; j >= 0; j--)
                {
                    bWithType = hrGroups[j].SelectMany(x => x.ChildNodes).FirstOrDefault(x => x.Name == "b");
                    if (bWithType != null) break;
                }

                var linkNodes = group.SelectMany(x => x.ChildNodes).Where(x => x.Name == "a").ToArray();

                var name = spanWithName?.GetAttributeValue("title", "").Trim() ?? "";
                var shortName = spanWithName?.InnerText.Trim() ?? "";
                var type = bWithType?.InnerText.Trim() ?? "";

                var teacher = linkNodes.Any() ? linkNodes[0].InnerText.Trim() : "";
                var location = linkNodes.Length < 3
                    ? ""
                    : linkNodes[1].InnerText.Trim() + '-' + linkNodes[2].InnerText.Trim();

                var calendarEvent = CreateEvent(dateTime, shortName, new[] {teacher, name}, location, name, type);

                events.Add(calendarEvent);
            }

            return events;
        }


        // ReSharper disable once ParameterTypeCanBeEnumerable.Local
        private static CalendarEvent CreateEvent(DateTime dateTime, string shortName, string[] teacher, string location,
            string name, string type)
        {
            return new CalendarEvent
            {
                Name = shortName,
                Contacts = new List<string>(teacher),
                Location = location,
                Description = $"Полное название: {name}\r\nТип: {type}",
                DtStart = new CalDateTime(dateTime),
                Duration = new TimeSpan(1, 35, 0)
            };
        }

        private static List<DayOfWeek> GetHolydays(IEnumerable<HtmlNode> rows)
        {
            var firstRowTds = rows
                .First(x => x.NodeType == HtmlNodeType.Element)
                .ChildNodes
                .Where(x => x.NodeType == HtmlNodeType.Element);

            var trimChars = new[] {' ', '\r', '\n'};
            var timeCellSkipped = firstRowTds.Skip(1);
            var result = timeCellSkipped
                .Select((node, index) => new
                {
                    IsHolyday = node.InnerText.Trim(trimChars) == "праздничный",
                    Index = index
                })
                .Where(x => x.IsHolyday)
                .Select(x => (DayOfWeek) (x.Index + 1))
                .ToList();

            return result;
        }
    }
}