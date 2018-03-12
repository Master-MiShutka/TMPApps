using System.Collections.Generic;

namespace TMP.UI.Controls.WPF.Reporting.MatrixGrid
{
    /// <summary>
    /// Интерфейс, описывающий заголовок матрицы <see cref="IMatrix"/>
    /// </summary>
    public interface IMatrixHeader : IMatrixItem
    {
        /// <summary>
        /// Ссылка на родительский заголовок
        /// </summary>
        IMatrixHeader Parent { get; }
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
    }
}