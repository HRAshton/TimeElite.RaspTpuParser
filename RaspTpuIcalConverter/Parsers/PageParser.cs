using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HRAshton.TimeElite.RaspTpuParser.Extensions;
using HRAshton.TimeElite.RaspTpuParser.Models;
using HtmlAgilityPack;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Calendar = Ical.Net.Calendar;

namespace HRAshton.TimeElite.RaspTpuParser.Parsers
{
    /// <summary>
    /// Парсер для html-страницы сайта rasp.tpu.ru.
    /// </summary>
    internal class PageParser
    {
        /// <summary>
        /// Преобразует строку с html-кодом страницы в объект типа <see cref="Calendar"/> (Ical.Net).
        /// </summary>
        /// У календаря есть поле Events, содержащее события. У события заполняются только атрибуты:
        /// <list type="bullet">
        /// <item> Name </item>
        /// <item> Categories </item>
        /// <item> Contacts </item>
        /// <item> Location </item>
        /// <item> Description </item>
        /// <item> DtStart </item>
        /// <item> DtEnd </item>
        /// <item> Duration </item>
        /// </list>
        /// <param name="htmlDocument">Документ со страницей Расписания.</param>
        /// <returns>Календарь с названием и событиями.</returns>
        public CalendarWithTimesModel ParsePage(HtmlDocument htmlDocument)
        {
            var tableNode = htmlDocument
                .GetElementbyId("raspisanie-table")
                .ChildNodes
                .FirstOrDefault(node => node.NodeType == HtmlNodeType.Element);

            var monday = GetMonday(tableNode);

            var name = ParseCalendarName(htmlDocument);

            var tbody = tableNode?.ChildNodes
                .LastOrDefault(node => node.NodeType == HtmlNodeType.Element); // tbody

            var calendar = GetCalendar(monday, tbody, name);

            return calendar;
        }

        private static string ParseCalendarName(HtmlDocument doc)
        {
            var title = doc.DocumentNode.SelectSingleNode("//head/title").InnerText
                            .Split("/")
                            .FirstOrDefault()
                        ?? "[ ?? ]";

            var name = title
                .Replace("«", string.Empty)
                .Replace("»", string.Empty);
            name = name.Replace("Расписание группы", string.Empty);
            name = name.Replace("Расписание для аудитории", string.Empty);
            name = name.Replace("Расписание для преподавателя", string.Empty);

            name = Regex.Replace(name, @"\s+", " ").Trim();

            return name;
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

        private CalendarWithTimesModel GetCalendar(DateTime mondayDate, HtmlNode tbody, string calendarName)
        {
            var rows = tbody.GetChildElementsList();

            var holidays = GetHolidays(rows);

            var tasks = new List<Task<RowParseResultModel>>();
            var results = new List<RowParseResultModel> { ParseRow(rows.First(), holidays, mondayDate, 0) };
            for (var rowIndex = 1; rowIndex < rows.Count; rowIndex++)
            {
                var index = rowIndex;
                var row = rows[index];
                var task = Task.Run(() => ParseRow(row, holidays, mondayDate, index + 1));
                tasks.Add(task);
            }

            Task.WhenAll(tasks).Result
                .ToList()
                .ForEach(results.Add);

            var calendar = new CalendarWithTimesModel
            {
                Name = calendarName,
                LessonsTimes = results
                    .Select(r => r.LessonStart)
                    .OrderBy(tuple => tuple.Hours).ThenBy(tuple => tuple.Minutes)
                    .ToList(),
            };
            calendar.Events.AddRange(
                results.SelectMany(r => r.Events));

            return calendar;
        }

        private RowParseResultModel ParseRow(
            HtmlNode row,
            ICollection<DayOfWeek> holidays,
            DateTime mondayDate,
            int rowIndex)
        {
            var lessonStartTime = GetStartTime(row);
            var mondayLessonStartDayTime = mondayDate.Add(lessonStartTime);
            var cells = row.GetChildElementsList();

            var events = new List<CalendarEvent>();

            var currentCellIndex = 0;
            for (var dayOfWeek = 0; dayOfWeek < 6; dayOfWeek++)
            {
                var currentDayOfWeek = (DayOfWeek)(dayOfWeek + 1);
                if (holidays.Contains(currentDayOfWeek))
                {
                    if (rowIndex == 0)
                    {
                        currentCellIndex++;
                    }

                    continue;
                }

                currentCellIndex++;
                var lessonStartDateTime = mondayLessonStartDayTime.AddDays(dayOfWeek);
                var cellEvents = ParseCell(cells[currentCellIndex], lessonStartDateTime);

                events.AddRange(cellEvents);
            }

            return new RowParseResultModel
            {
                Events = events,
                LessonStart = ((byte)lessonStartTime.Hours, (byte)lessonStartTime.Minutes),
            };
        }

        private static TimeSpan GetStartTime(HtmlNode row)
        {
            var lessonStartString = Regex.Match(row.GetChildElementsList().First().InnerText, @"\d{1,2}:\d{2}");
            var time = TimeSpan.Parse(lessonStartString.Value);

            return time;
        }

        private static IEnumerable<CalendarEvent> ParseCell(HtmlNode td, DateTime dateTime)
        {
            var locationRegex = new Regex(@"к. ([^\s]+), ауд. ([^\s]+)");

            var isEmptyCell = !td.ChildNodes.Any();
            if (isEmptyCell)
            {
                return new List<CalendarEvent>();
            }

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
                    spanWithName = hrGroups[j]
                        .SelectMany(x => x.ChildNodes)
                        .FirstOrDefault(x => x.Name == "span");
                    if (spanWithName != null)
                    {
                        break;
                    }
                }

                HtmlNode bWithType = null;
                for (var j = i; j >= 0; j--)
                {
                    bWithType = hrGroups[j]
                        .SelectMany(x => x.ChildNodes)
                        .FirstOrDefault(x => x.Name == "b");
                    if (bWithType != null)
                    {
                        break;
                    }
                }

                var name = spanWithName?.GetAttributeValue("title", string.Empty).Trim() ?? string.Empty;

                var shortName = spanWithName?.InnerText.Trim() ?? string.Empty;

                if (shortName == string.Empty)
                {
                    for (var j = i; j >= 0; j--)
                    {
                        shortName = new string(hrGroups[j]
                            .Select(x => x.InnerText)
                            .FirstOrDefault(x => x.Contains('('))
                            ?.TakeWhile(ch => ch != '(')
                            .SkipLast(1) // space
                            .ToArray() ?? Array.Empty<char>());

                        if (shortName != string.Empty)
                        {
                            break;
                        }
                    }
                }

                var type = bWithType?.InnerText.Trim() ?? string.Empty;
                var linkNodes = group.SelectMany(x => x.ChildNodes).Where(x => x.Name == "a").ToArray();
                var teacher = linkNodes.Any() ? linkNodes[0].InnerText.Trim() : string.Empty;

                var locationFragments = group.Select(x => locationRegex.Match(x.InnerText))
                    .FirstOrDefault(x => x.Success)
                    ?.Groups
                    .Values
                    .Skip(1)
                    .Select(x => x.Value)
                    .ToArray();
                var location = locationFragments != null
                    ? string.Join('-', locationFragments)
                    : string.Empty;

                var calendarEvent = new CalendarEvent
                {
                    Name = shortName,
                    Categories = new List<string> { type },
                    Contacts = new List<string> { teacher, name },
                    Location = location,
                    Description = $"Полное название: {name}\r\nТип: {type}",
                    DtStart = new CalDateTime(dateTime),
                    Duration = new TimeSpan(1, 35, 0),
                };

                events.Add(calendarEvent);
            }

            return events;
        }

        private static List<DayOfWeek> GetHolidays(IEnumerable<HtmlNode> rows)
        {
            var firstRowTds = rows
                .First(x => x.NodeType == HtmlNodeType.Element)
                .ChildNodes
                .Where(x => x.NodeType == HtmlNodeType.Element);

            var timeCellSkipped = firstRowTds.Skip(1);
            var result = timeCellSkipped
                .Select((cell, index) => new
                {
                    IsHolyday = cell.Attributes["rowspan"]?.Value == "7",
                    Index = index,
                })
                .Where(x => x.IsHolyday)
                .Select(x => (DayOfWeek)(x.Index + 1))
                .ToList();

            return result;
        }

        private class RowParseResultModel
        {
            /// <summary>
            ///     Время начала пары.
            /// </summary>
            public (byte Hours, byte Minutes) LessonStart { get; set; } = (0, 0);

            /// <summary>
            ///     Список пар в строке таблицы.
            /// </summary>
            public List<CalendarEvent> Events { get; set; } = new List<CalendarEvent>();
        }
    }
}