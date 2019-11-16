using System;
using System.Collections.Generic;
using System.Text;

namespace RaspTpuIcalConverter.RaspTpuModels
{
    internal class QueryResultModel
    {
        public List<QueryResultItemModel> Result { get; set; }
    }

    internal class QueryResultItemModel
    {
        public ushort Id { get; set; }
        public string Text { get; set; }
        public string Html { get; set; }
        public string Url { get; set; }
    }
}
