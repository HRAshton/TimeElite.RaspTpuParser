using System;

namespace HRAshton.TimeElite.RaspTpuParser.Models
{
    /// <summary>
    /// Модель результата поиска.
    /// </summary>
    [Serializable]
    internal class SearchResultModel
    {
        /// <summary>
        /// Ссылки на найденные разделы.
        /// </summary>
        public SearchResultItemModel[] Result { get; set; }
    }
}