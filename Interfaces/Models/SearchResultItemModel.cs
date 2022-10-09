using System;

namespace HRAshton.TimeElite.RaspTpuParser.Models
{
    /// <summary>
    /// Модель элемента результата поиска.
    /// </summary>
    [Serializable]
    public class SearchResultItemModel
    {
        /// <summary>
        /// Числовой идентификатор объекта.
        /// </summary>
        public uint Id { get; set; }

        /// <summary>
        /// Текст результата поиска.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Короткая ссылка.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Html-код для отображения результата поиска.
        /// </summary>
        public string Html { get; set; }
    }
}