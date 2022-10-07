using System.Collections.Generic;

namespace HRAshton.TimeElite.RaspTpuParser.RaspTpuModels
{
    /// <summary>
    /// Модель результата поиска.
    /// <example>https://rasp.tpu.ru/select/search/main.html?q=105</example>
    /// </summary>
    internal class QueryResultModel
    {
        /// <summary>
        /// Ссылки на найденные разделы.
        /// </summary>
        public IEnumerable<QueryResultItemModel> Result { get; set; }
    }
}