namespace RaspTpuIcalConverter.RaspTpuModels
{
    /// <summary>
    /// Модель Url расписания.
    /// </summary>
    internal class RaspUrlModel
    {
        /// <summary>
        /// Идентификатор.
        /// <example>pomeschenie_1960</example>
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Учебный год. При запросе в феврале будет всё так же возвращаться предыдущий год.
        /// <example>2019</example>
        /// </summary>
        public ushort Year { get; set; }

        /// <summary>
        /// Номер недели учебного года.
        /// <example>pomeschenie_1960</example>
        /// </summary>
        public byte Week { get; set; }

        /// <summary>
        /// Получить измененную Url по смещению номера недели.
        /// </summary>
        /// <param name="delta">Смещение номера недели.</param>
        /// <returns>Измененный Url.</returns>
        public string GetUrlForWeek(int delta)
        {
            return $"https://rasp.tpu.ru/{Id}/{Year}/{Week + delta}/view.html";
        }
    }
}