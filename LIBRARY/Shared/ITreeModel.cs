namespace TMP.Shared
{
    using System.Collections;

    /// <summary>
    /// Интерфейс определяющий модель данных для <see cref="TMP.UI.Controls.WPF.TreeListView.TreeListView"/>
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

    /// <summary>
    /// Интерфейс определяющий элемент иерархической структуры <see cref="TMP.Shared.ITreeModel"/>
    /// </summary>
    public interface ITreeNode : System.ComponentModel.INotifyPropertyChanged
    {
        /// <summary>
        /// Родитель элемента
        /// </summary>
        ITreeNode Parent { get; set; }

        /// <summary>
        /// Наименование
        /// </summary>
        string Header { get; set; }

        /// <summary>
        /// Перечень дочерних элементов
        /// </summary>
        System.Collections.Generic.ICollection<ITreeNode> Children { get; }

        /// <summary>
        /// Имеются ли дочерние элементы
        /// </summary>
        bool HasChildren { get; }

        /// <summary>
        /// Развёрнут ли элемент
        /// </summary>
        bool IsExpanded { get; set; }

        /// <summary>
        /// Подпадаёт ли элемент под критерий (поиск и т.п.)
        /// </summary>
        bool IsMatch { get; set; }
    }
}
