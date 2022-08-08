namespace TMP.ExcelOutput
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    public class ExcelOutputCollection : ICollection<ExcelOutputItem>
    {
        public PostCreationActions PostCreationAction { get; }

        public string Path { get; }

        private readonly Collection<ExcelOutputItem> collection;

        /// <summary>
        /// Instantiate an instance of the ExcelOutputCollection with default values.
        /// This collection allows for the creation of multiple sheets within a single Workbook.
        /// </summary>
        public ExcelOutputCollection()
        {
            this.collection = new Collection<ExcelOutputItem>();
            this.PostCreationAction = PostCreationActions.Open;
            this.Path = null;
        }

        /// <summary>
        /// Instantiate an instance of the ExcelOutputCollection
        /// This collection allows for the creation of multiple sheets within a single Workbook.
        /// <param name="postCreationAction">Determines whether the Excel file will be opened with the data, opened and saved or just saved.</param>
        /// <param name="path">If the path parameter determines where the Excel file will be saved to if a save action is selected from the <see cref="PostCreationActions"/>.</param>
        /// </summary>
        public ExcelOutputCollection(PostCreationActions postCreationAction, string path)
        {
            this.collection = new Collection<ExcelOutputItem>();
            this.PostCreationAction = postCreationAction;
            this.Path = path;
        }

        public IEnumerator<ExcelOutputItem> GetEnumerator()
        {
            return this.collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void Add(ExcelOutputItem item)
        {
            if (this.collection.Any(x => x.WorksheetName == item.WorksheetName))
            {
                throw new WorksheetNameExistsException();
            }

            this.collection.Add(item);
        }

        public void Add(params ExcelOutputItem[] items)
        {
            foreach (var item in items)
            {
                this.Add(item);
            }
        }

        public void Add(IEnumerable<ExcelOutputItem> items)
        {
            foreach (var item in items)
            {
                this.Add(item);
            }
        }

        public void Clear()
        {
            this.collection.Clear();
        }

        public bool Contains(ExcelOutputItem item)
        {
            return this.collection.Contains(item);
        }

        public bool Contains(string worksheetName)
        {
            return this.collection.Any(x => x.WorksheetName == worksheetName);
        }

        public void CopyTo(ExcelOutputItem[] array, int arrayIndex)
        {
            this.collection.CopyTo(array, arrayIndex);
        }

        public bool Remove(ExcelOutputItem item)
        {
            return this.collection.Remove(item);
        }

        public int Count => this.collection.Count;

        public bool IsReadOnly => false;
    }
}
