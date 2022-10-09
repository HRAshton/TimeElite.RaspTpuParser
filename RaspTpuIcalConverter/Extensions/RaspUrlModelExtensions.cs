using HRAshton.TimeElite.RaspTpuParser.Helpers;
using HRAshton.TimeElite.RaspTpuParser.Models;

namespace HRAshton.TimeElite.RaspTpuParser.Extensions
{
    /// <summary>
    /// Расширения для метаданных расписания.
    /// </summary>
    internal static class RaspUrlModelExtensions
    {
        /// <summary>
        /// Получить измененную Url по смещению номера недели.
        /// </summary>
        /// <param name="raspUrlModel">Модель Url базового расписания.</param>
        /// <param name="delta">Смещение номера недели.</param>
        /// <returns>Измененный Url.</returns>
        public static string GetUrlForWeek(this RaspUrlModel raspUrlModel, int delta)
        {
            return RaspTpuUrlHelper.CreateForWeek(raspUrlModel.Id, raspUrlModel.Year, raspUrlModel.Week, delta);
        }
    }
}