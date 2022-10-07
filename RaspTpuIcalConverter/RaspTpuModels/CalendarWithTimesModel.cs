using System.Collections.Generic;
using Ical.Net;

namespace RaspTpuIcalConverter.RaspTpuModels
{
    /// <summary>
    /// Модель результата парсера расписания.
    /// </summary>
    public class CalendarWithTimesModel : Calendar
    {
        /// <summary>
        /// Список времени начала пар.
        /// </summary>
        public List<(byte Hours, byte Minutes)> LessonsTimes { get; set; } = new List<(byte, byte)>();
    }
}