namespace TMP.Shared.Common.Tree
{
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Интерфейс определяющий модель данных для <see cref="TMP.UI.WPF.Controls.TreeListView.TreeListView"/>
    /// </summary>
    public interface ITreeModel
    {
        /// <summary>
        /// Возвращает список дочерних элементов указанного родителя
        /// </summary>
        IEnumerable GetParentChildren(ITreeNode parent);

        /// <summary>
        /// Возвращает имеются ли у указанного родителя дочерние элементы
        /// </summary>
        bool HasParentChildren(ITreeNode parent);
    }
}
