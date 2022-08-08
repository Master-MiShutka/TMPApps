using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TMP.Work.AmperM.TestApp.Model
{
    using Shared;
    /// <summary>
    /// Представляет группу элементов в хранилище запросов
    /// </summary>
    public class RepositoryGroup : IRepositoryItem, INotifyPropertyChanged
    {
        private Int64 _id;
        private DateTime _date;
        private ObservableCollection<IRepositoryItem> _items;
        private string _title;
        private string _description;
        /// <summary>
        /// Тип элемента
        /// </summary>
        public RepositoryItemType Type { get { return RepositoryItemType.Group; } }
        /// <summary>
        /// Идентификатор
        /// </summary>
        public Int64 ID { get { return _id; } set { SetProperty(ref _id, value); } }
        /// <summary>
        /// Дата добавления
        /// </summary>
        public DateTime AddedDate { get { return _date; } private set { SetProperty(ref _date, value); } }
        /// <summary>
        /// Колекция подчиненных элементов
        /// </summary>
        public ObservableCollection<IRepositoryItem> Items { get { return _items; } set { SetProperty(ref _items, value); } }
        /// <summary>
        /// Название
        /// </summary>
        public string Title { get { return _title; } set { SetProperty(ref _title, value); } }
        /// <summary>
        /// Описание
        /// </summary>
        public string Description { get { return _description; } set { SetProperty(ref _description, value); } }
        /// <summary>
        /// Владелец элемента в дереве
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public IRepositoryItem Parent { get; set; }
        public RepositoryGroup()
        {
            AddedDate = DateTime.Today;
            Items = new ObservableCollection<IRepositoryItem>();
        }
        /// <summary>
        /// Возвращает копию объекта
        /// </summary>
        public IRepositoryItem Clone()
        {
            return new RepositoryGroup
            {
                AddedDate = this.AddedDate,
                Items = this.Items,
                Title = this.Title,
                Description = this.Description,
                Parent = this.Parent
            };
        }
        /// <summary>
        /// Обновление элемента на основании переданного
        /// </summary>
        public void Update(IRepositoryItem item)
        {
            this.Title = item.Title;
            this.Description = item.Description;
        }
        public override string ToString()
        {
            if (String.IsNullOrEmpty(Title))
                return "пустой";
            else
                return Type + " - " + Title + (Items.Count > 0 ? Items.Count + " дочерних элементов" : "нет дочерних элементов");
        }
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        public bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return false;
            }

            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        #endregion // INotifyPropertyChanged Members
    }
}
