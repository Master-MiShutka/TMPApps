using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using TMP.Shared.Commands;

namespace TMP.Work.AmperM.TestApp
{
  using MsgBox;
  public class PagingCollectionView : ListCollectionView
  {
    private int _itemsPerPage;
    private int _currentPage = 1;

    private readonly ICommand _moveFirstCommand, _moveToPreviousCommand, _moveToNextCommand, _moveToLastCommand;
    private readonly ICommand _moveFirstPageCommand, _moveToPreviousPageCommand, _moveToNextPageCommand, _moveToLastPageCommand;

    public PagingCollectionView(IList list, int itemsPerPage)
        : base(list)
    {
      this._itemsPerPage = itemsPerPage;
      this.TotalItemsCountExcludingFilter = list.Count;      

      this.ShowAllItems = new DelegateCommand(() =>
      {
        if (base.InternalList == null) return;
        if (Count > 3000)
        {
          if (MessageBox.Show("Отображение всех записей сразу может замедлить\nработу компьютера. Продолжить?", App.WindowTitle,
                MsgBoxButtons.YesNo, MsgBoxImage.Warning) == MsgBoxResult.Yes)
            ItemsPerPage = base.InternalCount;
        }
        else
          ItemsPerPage = base.InternalCount;
      },
      (o) => base.InternalList != null && _itemsPerPage >= base.InternalCount);

      _moveFirstCommand = new DelegateCommand(() =>
      {
        MoveCurrentToFirst();
        NotifyToUpdateProperties();
      },
      (o) => base.InternalList != null && UICurrentItemPositionOnPage > 1);
      _moveToPreviousCommand = new DelegateCommand(() =>
      {
        MoveCurrentToPrevious();
        NotifyToUpdateProperties();
      },
      (o) => base.InternalList != null && UICurrentItemPositionOnPage > 1);
      _moveToNextCommand = new DelegateCommand(() =>
      {
        MoveCurrentToNext();
        NotifyToUpdateProperties();
        if (IsCurrentAfterLast)
          MoveCurrentToLast();
      }, (o) => base.InternalList != null && !IsCurrentAfterLast && UICurrentItemPositionOnPage < Count);
      _moveToLastCommand = new DelegateCommand(() =>
      {
        MoveCurrentToLast();
        NotifyToUpdateProperties();
      },
      (o) => base.InternalList != null && !IsCurrentAfterLast && UICurrentItemPositionOnPage < Count);

      _moveFirstPageCommand = new DelegateCommand(() =>
      {
        MoveToFirstPage();
        NotifyToUpdateProperties();
      },
      (o) => CurrentPage > 1);
      _moveToPreviousPageCommand = new DelegateCommand(() =>
      {
        MoveToPreviousPage();
        NotifyToUpdateProperties();
      },
      (o) => CurrentPage > 1);
      _moveToNextPageCommand = new DelegateCommand(() =>
      {
        MoveToNextPage();
        NotifyToUpdateProperties();
      },
      (o) => CurrentPage < PageCount);
      _moveToLastPageCommand = new DelegateCommand(() =>
      {
        MoveToLastPage();
        NotifyToUpdateProperties();
      },
      (o) => CurrentPage < PageCount);

      base.PropertyChanged += CollectionView_PropertyChanged;
    }

    public PagingCollectionView(IList list) : this(list, 25)
    {
      ;
    }

    private void CollectionView_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      switch (e.PropertyName)
      {
        case "CurrentPosition":
          this.OnPropertyChanged(new PropertyChangedEventArgs("UICurrentItemPositionOnPage"));
          break;
        case "Count":
          this.OnPropertyChanged(new PropertyChangedEventArgs("IsFitering"));
          this.OnPropertyChanged(new PropertyChangedEventArgs("TotalItemsCountIncludingFilter"));
          this.OnPropertyChanged(new PropertyChangedEventArgs("TotalItemsCountExcludingFilter"));

          this.OnPropertyChanged(new PropertyChangedEventArgs("UIStartIndex"));
          this.OnPropertyChanged(new PropertyChangedEventArgs("UIEndIndex"));

          this.OnPropertyChanged(new PropertyChangedEventArgs("CurrentPage"));
          this.OnPropertyChanged(new PropertyChangedEventArgs("PageCount"));
          break;
        default:
          break;
      }
    }
    public int TotalItemsCountExcludingFilter { get; private set; }
    public int TotalItemsCountIncludingFilter
    {
      get { return this.InternalCount; }
    }
    public bool IsFitering { get { return TotalItemsCountExcludingFilter != TotalItemsCountIncludingFilter; } }

    public int ItemsPerPage
    {
      get { return _itemsPerPage; }
      set
      {
        _itemsPerPage = value;
        this.OnPropertyChanged(new PropertyChangedEventArgs("ItemsPerPage"));
        this.OnPropertyChanged(new PropertyChangedEventArgs("UIItemsCountOnPage"));
        this.OnPropertyChanged(new PropertyChangedEventArgs("CurrentPage"));
        this.OnPropertyChanged(new PropertyChangedEventArgs("PageCount"));
        NotifyToUpdateProperties();
        this.Refresh();
      }
    }
    public int UICurrentItemPositionOnPage
    {
      get
      {
        return base.CurrentPosition + 1;
      }
    }
    public int UIItemsCountOnPage { get { return Count; } }
    public override int Count
    {
      get
      {
        if (base.InternalList == null) return 0;
        if (this.InternalCount == 0) return 0;
        if (this.CurrentPage < this.PageCount) // page 1..n-1
        {
          return this._itemsPerPage;
        }
        else // page n
        {
          var itemsLeft = this.InternalCount % this._itemsPerPage;
          if (0 == itemsLeft)
          {
            return this._itemsPerPage; // exactly itemsPerPage left
          }
          else
          {
            // return the remaining items
            return itemsLeft;
          }
        }
      }
    }
    public int UIStartIndex
    {
      get
      {
        return this.StartIndex + 1;
      }
    }
    public int UIEndIndex
    {
      get
      {
        return (EndIndex >= this.InternalCount) ? this.InternalCount : EndIndex + 1;
      }
    }

    public int CurrentPage
    {
      get
      {
        if (this._currentPage > this.PageCount)
          this._currentPage = 1;
        return this._currentPage;
      }
      set
      {
        this._currentPage = value;
        this.OnPropertyChanged(new PropertyChangedEventArgs("CurrentPage"));
      }
    }

    public int PageCount
    {
      get
      {
        return (this.InternalCount + this._itemsPerPage - 1) / this._itemsPerPage;
      }
    }

    public int EndIndex
    {
      get
      {
        var end = this.CurrentPage * this._itemsPerPage - 1;
        return (end > this.InternalCount) ? this.InternalCount : end;
      }
    }

    public int StartIndex
    {
      get { return (this.CurrentPage - 1) * this._itemsPerPage; }
    }
    #region Public Methods
    public override object GetItemAt(int index)
    {
      var offset = index % (this._itemsPerPage);
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
    public ICommand MoveToFirstCommand
    {
      get { return _moveFirstCommand; }
    }

    public ICommand MoveToLastCommand
    {
      get { return _moveToLastCommand; }
    }

    public ICommand MoveToNextCommand
    {
      get { return _moveToNextCommand; }
    }

    public ICommand MoveToPreviousCommand
    {
      get { return _moveToPreviousCommand; }
    }
    #endregion
    #region Команды навигации по страницам
    public ICommand MoveToFirstPageCommand
    {
      get { return _moveFirstPageCommand; }
    }

    public ICommand MoveToLastPageCommand
    {
      get { return _moveToLastPageCommand; }
    }

    public ICommand MoveToNextPageCommand
    {
      get { return _moveToNextPageCommand; }
    }

    public ICommand MoveToPreviousPageCommand
    {
      get { return _moveToPreviousPageCommand; }
    }
    #endregion

    #endregion

    private void NotifyToUpdateProperties()
    {
      this.OnPropertyChanged(new PropertyChangedEventArgs("UIStartIndex"));
      this.OnPropertyChanged(new PropertyChangedEventArgs("UIEndIndex"));
    }
  }
}