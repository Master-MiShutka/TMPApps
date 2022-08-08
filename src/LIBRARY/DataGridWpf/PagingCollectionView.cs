namespace DataGridWpf
{
    using System.Collections;
    using System.ComponentModel;
    using System.Windows.Data;
    using System.Windows.Input;
    using TMP.Shared.Common.Commands;

    public class PagingCollectionView : ListCollectionView
    {
        private int itemsPerPage;
        private int currentPage = 1;

        private readonly ICommand _moveFirstCommand;
        private readonly ICommand _moveToPreviousCommand;
        private readonly ICommand _moveToNextCommand;
        private readonly ICommand _moveToLastCommand;
        private readonly ICommand _moveFirstPageCommand;
        private readonly ICommand _moveToPreviousPageCommand;
        private readonly ICommand _moveToNextPageCommand;
        private readonly ICommand _moveToLastPageCommand;

        public PagingCollectionView(IList list, int itemsPerPage)
            : base(list)
        {
            this.itemsPerPage = itemsPerPage;
            this.TotalItemsCountExcludingFilter = list.Count;

            this.ShowAllItems = new DelegateCommand(() =>
            {
                if (this.InternalList == null)
                {
                    return;
                }

                if (this.Count > 3000)
                {
                    this.ItemsPerPage = this.InternalCount;
                }
                else
                {
                    this.ItemsPerPage = this.InternalCount;
                }
            },
            () => this.InternalList != null && this.itemsPerPage < this.InternalCount);

            this._moveFirstCommand = new DelegateCommand(() =>
            {
                this.MoveCurrentToFirst();
                this.NotifyToUpdateProperties();
            },
            () => this.InternalList != null && this.UICurrentItemPositionOnPage > 1);

            this._moveToPreviousCommand = new DelegateCommand(() =>
            {
                this.MoveCurrentToPrevious();
                this.NotifyToUpdateProperties();
            },
            () => this.InternalList != null && this.UICurrentItemPositionOnPage > 1);

            this._moveToNextCommand = new DelegateCommand(() =>
            {
                this.MoveCurrentToNext();
                this.NotifyToUpdateProperties();
                if (this.IsCurrentAfterLast)
                {
                    this.MoveCurrentToLast();
                }
            }, () => this.InternalList != null && !this.IsCurrentAfterLast && this.UICurrentItemPositionOnPage < this.Count);

            this._moveToLastCommand = new DelegateCommand(() =>
            {
                this.MoveCurrentToLast();
                this.NotifyToUpdateProperties();
            },
            () => this.InternalList != null && !this.IsCurrentAfterLast && this.UICurrentItemPositionOnPage < this.Count);

            this._moveFirstPageCommand = new DelegateCommand(() =>
            {
                this.MoveToFirstPage();
                this.NotifyToUpdateProperties();
            },
            () => this.CurrentPage > 1);

            this._moveToPreviousPageCommand = new DelegateCommand(() =>
            {
                this.MoveToPreviousPage();
                this.NotifyToUpdateProperties();
            },
            () => this.CurrentPage > 1);

            this._moveToNextPageCommand = new DelegateCommand(() =>
            {
                this.MoveToNextPage();
                this.NotifyToUpdateProperties();
            },
            () => this.CurrentPage < this.PageCount);

            this._moveToLastPageCommand = new DelegateCommand(() =>
            {
                this.MoveToLastPage();
                this.NotifyToUpdateProperties();
            },
            () => this.CurrentPage < this.PageCount);

            this.PropertyChanged += this.CollectionView_PropertyChanged;
        }

        public PagingCollectionView(IList list) 
            : this(list, 25)
        {
        }

        private void CollectionView_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "CurrentPosition":
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.UICurrentItemPositionOnPage)));

                    this.NotifyToUpdateProperties();
                    break;
                case "Count":
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.IsFitering)));
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.TotalItemsCountIncludingFilter)));
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.TotalItemsCountExcludingFilter)));

                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.UIStartIndex)));
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.UIEndIndex)));

                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.CurrentPage)));
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.PageCount)));

                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.PageCount)));

                    break;
                default:
                    break;
            }
        }

        public int TotalItemsCountExcludingFilter { get; private set; }

        public int TotalItemsCountIncludingFilter => this.InternalCount;

        public bool IsFitering => this.TotalItemsCountExcludingFilter != this.TotalItemsCountIncludingFilter;

        public int ItemsPerPage
        {
            get => this.itemsPerPage;

            set
            {
                this.itemsPerPage = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.ItemsPerPage)));

                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Count)));
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.StartIndex)));
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.EndIndex)));
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.UIStartIndex)));
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.UIEndIndex)));
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.UIItemsCountOnPage)));
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.CurrentPage)));
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.PageCount)));
                this.NotifyToUpdateProperties();
                this.Refresh();
            }
        }

        public int UICurrentItemPositionOnPage => this.CurrentPosition + 1;

        public int UIItemsCountOnPage => (this.UIEndIndex < this.Count) ? this.Count : this.Count;

        public override int Count
        {
            get
            {
                if (this.InternalList == null)
                {
                    return 0;
                }

                if (this.InternalCount == 0)
                {
                    return 0;
                }

                if (this.CurrentPage < this.PageCount) // page 1..n-1
                {
                    return this.itemsPerPage;
                }
                else // page n
                {
                    var itemsLeft = this.InternalCount % this.itemsPerPage;
                    if (itemsLeft == 0)
                    {
                        return this.itemsPerPage; // exactly itemsPerPage left
                    }
                    else
                    {
                        // return the remaining items
                        return itemsLeft;
                    }
                }
            }
        }

        public int UIStartIndex => this.StartIndex + 1;

        public int UIEndIndex => (this.EndIndex >= this.InternalCount) ? this.InternalCount : this.EndIndex + 1;

        public int CurrentPage
        {
            get
            {
                if (this.currentPage > this.PageCount)
                {
                    this.currentPage = 1;
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.CurrentPage)));
                }

                return this.currentPage;
            }

            set
            {
                this.currentPage = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.CurrentPage)));
            }
        }

        public int PageCount => (this.InternalCount + this.itemsPerPage - 1) / this.itemsPerPage;

        public int EndIndex
        {
            get
            {
                var end = (this.CurrentPage * this.itemsPerPage) - 1;
                return (end > this.InternalCount) ? this.InternalCount : end;
            }
        }

        public int StartIndex => (this.CurrentPage - 1) * this.itemsPerPage;

        #region Public Methods
        public override object GetItemAt(int index)
        {
            var offset = index % this.itemsPerPage;
            return this.InternalList[this.StartIndex + offset];
        }

        public void MoveToNextPage()
        {
            if (this.CurrentPage < this.PageCount)
            {
                this.CurrentPage += 1;
            }

            this.Refresh();
        }

        public void MoveToLastPage()
        {
            if (this.CurrentPage < this.PageCount)
            {
                this.CurrentPage = this.PageCount - 1;
            }

            this.Refresh();
        }

        public void MoveToPreviousPage()
        {
            if (this.CurrentPage > 1)
            {
                this.CurrentPage -= 1;
            }

            this.Refresh();
        }

        public void MoveToFirstPage()
        {
            if (this.CurrentPage > 1)
            {
                this.CurrentPage = 1;
            }

            this.Refresh();
        }

        #endregion

        #region Commands
        public ICommand ShowAllItems { get; private set; }

        #region Команды навигации по элементам страницы
        public ICommand MoveToFirstCommand => this._moveFirstCommand;

        public ICommand MoveToLastCommand => this._moveToLastCommand;

        public ICommand MoveToNextCommand => this._moveToNextCommand;

        public ICommand MoveToPreviousCommand => this._moveToPreviousCommand;

        #endregion
        #region Команды навигации по страницам
        public ICommand MoveToFirstPageCommand => this._moveFirstPageCommand;

        public ICommand MoveToLastPageCommand => this._moveToLastPageCommand;

        public ICommand MoveToNextPageCommand => this._moveToNextPageCommand;

        public ICommand MoveToPreviousPageCommand => this._moveToPreviousPageCommand;
        #endregion

        #endregion

        private void NotifyToUpdateProperties()
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.UIStartIndex)));
            this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.UIEndIndex)));
            this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.UICurrentItemPositionOnPage)));

            (this.MoveToPreviousCommand as DelegateCommand)?.RaiseCanExecuteChanged();
            (this.MoveToNextCommand as DelegateCommand)?.RaiseCanExecuteChanged();

            (this.MoveToFirstCommand as DelegateCommand)?.RaiseCanExecuteChanged();
            (this.MoveToLastCommand as DelegateCommand)?.RaiseCanExecuteChanged();

            (this.MoveToFirstPageCommand as DelegateCommand)?.RaiseCanExecuteChanged();
            (this.MoveToLastPageCommand as DelegateCommand)?.RaiseCanExecuteChanged();

            (this.MoveToNextPageCommand as DelegateCommand)?.RaiseCanExecuteChanged();
            (this.MoveToPreviousPageCommand as DelegateCommand)?.RaiseCanExecuteChanged();

            (this.ShowAllItems as DelegateCommand)?.RaiseCanExecuteChanged();
        }
    }
}
