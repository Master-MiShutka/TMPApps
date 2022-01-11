namespace TMP.Shared
{
    using System.Collections;

    public interface ITreeModel
    {
        /// <summary>
        /// Возвращает список дочерних элементов указанного родителя
        /// </summary>
        IEnumerable GetParentChildren(object parent);

        /// <summary>
        /// возвращает имеются ли у указанного родителя дочерние элементы
        /// </summary>
        bool HasParentChildren(object parent);
    }
}
