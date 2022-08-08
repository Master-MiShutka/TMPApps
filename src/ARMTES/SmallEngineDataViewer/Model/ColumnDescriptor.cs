using System.Collections;
using System.Collections.Generic;

namespace TMP.ARMTES
{
    /// <summary>
    /// Класс, описывающий колонку таблицы <see cref="ColumnDescriptor" />
    /// </summary>
    public class ColumnDescriptor
    {
        /// <summary>
        /// Заголовок колонки
        /// </summary>
        public string HeaderText { get; set; }

        /// <summary>
        /// Имя поля
        /// </summary>
        public string DisplayMember { get; set; }

        public IEnumerable<ColumnDescriptor> ChildColumns { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnDescriptor"/> class.
        /// </summary>
        /// <param name="header">Заголовок <see cref="string"/></param>
        /// <param name="displayMember">Имя поля <see cref="string"/></param>
        public ColumnDescriptor(string header, string displayMember)
        {
            HeaderText = header;
            DisplayMember = displayMember;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnDescriptor"/> class.
        /// </summary>
        /// <param name="header">Заголовок <see cref="string"/></param>
        /// <param name="displayMember">Имя поля <see cref="string"/></param>
        /// <param name="childColumns">Список подколонок <see cref="IEnumerable<ColumnDescriptor>"/></param>
        public ColumnDescriptor(string header, string displayMember, IEnumerable<ColumnDescriptor> childColumns) : this(header, displayMember)
        {
            ChildColumns = childColumns;
        }
    }
}
