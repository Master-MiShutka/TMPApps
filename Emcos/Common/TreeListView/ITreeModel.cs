using System.Collections;

namespace TMP.Wpf.Common.Controls.TreeListView
{
    public interface ITreeModel
    {
        /// <summary>
        /// Возвращает список дочерних элементов указанного родителя
        /// </summary>
        IEnumerable GetChildren(object parent);

        /// <summary>
        /// возвращает имеются ли у указанного родителя дочерние элементы
        /// </summary>
        bool HasChildren(object parent);
    }
}
