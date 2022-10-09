using System;
using System.Collections.Generic;
using HRAshton.TimeElite.RaspTpuParser.Extensions;
using HRAshton.TimeElite.RaspTpuParser.Models;

namespace HRAshton.TimeElite.RaspTpuParser.Helpers
{
    /// <summary>
    /// Набор функций для работы с Url и сетью.
    /// </summary>
    internal static class UrlHelper
    {
        /// <summary>
        /// Проверяет, является ли строка абсолютным Url.
        /// </summary>
        /// <param name="url">Строка, содержащая url.</param>
        /// <returns>Является ли строка абсолютным Url.</returns>
        public static bool IsAbsoluteUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }

        /// <summary>
        /// Получить коллекцию адресов страниц с расписанием по адресу и отступам вперед и назад по дате.
        /// </summary>
        /// <param name="link">Адрес страницы с базовым расписанием.</param>
        /// <param name="before">Количество календарей до переданного, события которых нужно включить.</param>
        /// <param name="skipBetweenCurrentWeekAndAfter">
        /// Количество недель (включая текущую), которые должны быть пропущены.
        /// </param>
        /// <param name="after">Количество календарей после переданного, события которых нужно включить.</param>
        /// <returns>Коллекция адресов с расписаниями.</returns>
        public static ICollection<string> CreateUrls(
            string link,
            byte before,
            byte skipBetweenCurrentWeekAndAfter,
            byte after)
        {
            var urls = new HashSet<string> { link };

            var isAnyModifierSet = before != 0 || after != 0 || skipBetweenCurrentWeekAndAfter != 0;
            if (!isAnyModifierSet)
            {
                return urls;
            }

            var parsedUrl = ParseUrl(link);

            for (var i = 1; i <= before; i++)
            {
                urls.Add(parsedUrl.GetUrlForWeek(-i));
            }

            for (var i = skipBetweenCurrentWeekAndAfter + 1; i <= after; i++)
            {
                urls.Add(parsedUrl.GetUrlForWeek(i));
            }

            return urls;
        }

        /// <summary>
        /// Получить метаданные о расписании по фрагментам адреса страницы.
        /// </summary>
        /// <param name="url">Адрес страницы расписания.</param>
        /// <returns>Метаданные о расписании.</returns>
        private static RaspUrlModel ParseUrl(string url)
        {
            var fragments = url.Split('/');
            if (fragments.Length != 7)
            {
                throw new ArgumentException("Invalid rasp.tpu.ru url.");
            }

            var result = new RaspUrlModel
            {
                Id = fragments[3],
                Year = ushort.Parse(fragments[4]),
                Week = byte.Parse(fragments[5]),
            };

            return result;
        }
    }
}