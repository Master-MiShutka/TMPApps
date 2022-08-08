namespace TMP.UI.WPF.Reporting.MatrixGrid
{
    using System.Collections.Generic;

    /// <summary>
    /// Интерфейс, описывающий заголовок матрицы <see cref="IMatrix"/>
    /// </summary>
    public interface IMatrixHeader : IMatrixCell
    {
        /// <summary>
        /// Заголовок
        /// </summary>
        string Header { get; }

        /// <summary>
        /// Объект, хранящий дополнительную информацию
        /// </summary>
        object Tag { get; }

        /// <summary>
        /// Список подзаголовков
        /// </summary>
        IList<IMatrixHeader> Children { get; }

        /// <summary>
        /// Количество подзаголовков
        /// </summary>
        int ChildrenCount { get; }

        /// <summary>
        /// Ссылка на родителя
        /// </summary>
        IMatrixHeader Parent { get; set; }
    }
}