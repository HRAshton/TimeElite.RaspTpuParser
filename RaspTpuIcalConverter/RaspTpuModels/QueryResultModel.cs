using System.Collections.Generic;

namespace RaspTpuIcalConverter.RaspTpuModels
{
    /// <summary>
    ///     Модель результата поиска.
    ///     <example>https://rasp.tpu.ru/select/search/main.html?q=105</example>
    /// </summary>
    internal class QueryResultModel
    {
        public List<QueryResultItemModel> Result { get; set; }
    }
}