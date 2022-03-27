namespace TMP.Shared.Tree
{
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Интерфейс определяющий элемент иерархической структуры <see cref="TMP.Shared.ITreeModel"/>
    /// </summary>
    public interface ITreeNode : System.ComponentModel.INotifyPropertyChanged
    {
        /// <summary>
        /// Уровень элемента
        /// </summary>
        public int Level { get; }

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
        System.Collections.Generic.IList<ITreeNode> Children { get; }

        /// <summary>
        /// Имеются ли дочерние элементы
        /// </summary>
        bool HasChildren { get; }

        /// <summary>
        /// Может ли элемент разворачиваться
        /// </summary>
        bool IsExpandable { get; set; }

        /// <summary>
        /// Развёрнут ли элемент
        /// </summary>
        bool IsExpanded { get; set; }

        /// <summary>
        /// Подпадаёт ли элемент под критерий (поиск и т.п.)
        /// </summary>
        bool IsMatch { get; set; }

        /// <summary>
        /// Apply criteria for node
        /// </summary>
        /// <param name="criteria">String</param>
        /// <param name="ancestors">Stack of parents nodes</param>
        void ApplyCriteria(string criteria, Stack<ITreeNode> ancestors);
    }
}
