using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;

namespace TMP.Work.AmperM.TestApp.DataAccess
{
  using Model;
  using Common.RepositoryCommon;
  using System.Runtime.Serialization;
  using MsgBox;

  public class RequestsRepository : ViewModel.AbstractViewModel
  {
    #region Fields
    public static readonly string REPOSITORY_FILE_EXTENSION = ".repository";
    public static readonly string REPOSITORY_FILE_NAME = "main" + REPOSITORY_FILE_EXTENSION;
    private ObservableCollection<IRepositoryItem> _repositoryItems;
    #endregion

    #region Constructor
    public RequestsRepository(string filleName = null)
    {
      jsonSettings = new Newtonsoft.Json.JsonSerializerSettings
      {
        TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
        Binder = binder
      };
      //BaseRepository<List<IRepositoryItem>>.JsonSerializerSettings = jsonSettings;

      Load(filleName);
    }
    #endregion
    #region Properties

    public ObservableCollection<IRepositoryItem> RepositoryItems
    {
      get { return _repositoryItems; }
      set
      {
        SetProperty(ref _repositoryItems, value);
      }
    }

    #endregion

    #region Load and Save
    public void Load(string fileName = null)
    {
      if (fileName == null)
        if (String.IsNullOrEmpty(Properties.Settings.Default.DataFileName))
          fileName = REPOSITORY_FILE_NAME;
        else
          fileName = Properties.Settings.Default.DataFileName;
      App.Log.Log(String.Format("Чтение репозитория из файла <{0}>", fileName));
      fileName = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Application.ResourceAssembly.Location), REPOSITORY_FILE_NAME);
      IList<IRepositoryItem> list = BaseRepository<List<IRepositoryItem>>.GzJsonDeSerialize(
          fileName,
          (e) =>
          {
            App.LogException(e);
            MessageBox.Show(
                      String.Format("Не удалось загрузить список запросов.\n\t{0}", e.Message),
                      "ОШИБКА", MsgBoxButtons.OK, MsgBoxImage.Error);
            return;
          });
      InitRepositoryItemsList(ref list);
      RepositoryItems = new ObservableCollection<IRepositoryItem>(list);
    }
    public void Save(string fileName = null)
    {
      if (fileName == null)
        fileName = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Application.ResourceAssembly.Location), REPOSITORY_FILE_NAME);
      App.Log.Log(String.Format("Запись репозитория в файл <{0}>", fileName));
      BaseRepository<List<IRepositoryItem>>.GzJsonSerialize(
          _repositoryItems.ToList(),
          fileName,
          (e) =>
          {
            App.LogException(e);
            MessageBox.Show(
                      String.Format("Не удалось сохранить список запросов.\n\t{0}", e.Message),
                      "ОШИБКА", MsgBoxButtons.OK, MsgBoxImage.Error);
          });
    }

    #endregion

    #region Public

    public event EventHandler<RepositoryItemAddedEventArgs> RepositoryItemAdded;

    public bool AddRepositoryItem(IRepositoryItem item)
    {
      if (item == null)
        throw new ArgumentNullException("item");

      if (!_repositoryItems.Contains(item))
      {
        App.Log.Log(String.Format("Добавление элемента в репозиторий: <{0}>", item.ToString()));

        RepositoryItems.Add(item);

        if (RepositoryItemAdded != null)
          RepositoryItemAdded(this, new RepositoryItemAddedEventArgs(item));
        return true;
      }
      else return false;
    }
    /// <summary>
    /// Возвращает копию списка элементов репозитория
    /// </summary>
    public List<IRepositoryItem> GetItems()
    {
      return new List<IRepositoryItem>(_repositoryItems);
    }

    #endregion
    #region Private Helpers

    private Newtonsoft.Json.JsonSerializerSettings jsonSettings;
    private SerializationBinder binder = new TypeNameSerializationBinder("Test.0, Test");


    private static void InitRepositoryItemsList(ref IList<IRepositoryItem> items)
    {
      if (items != null)
      {
        SetParent(items, null);
        //AddChildCollectionListeners(items);
      }
      else
      {
        items = new List<IRepositoryItem>();
      }
    }
    private static void SetParent(ICollection<IRepositoryItem> items, IRepositoryItem parent)
    {
      foreach (IRepositoryItem item in items)
      {
        item.Parent = parent;
        SetParent(item.Items, item);
      }
    }
    private static void AddChildCollectionListeners(IList<IRepositoryItem> collection)
    {
      foreach (var item in collection)
      {
        item.Items.CollectionChanged += Items_CollectionChanged;
        AddChildCollectionListeners(item.Items);
      }
    }

    private static void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
      System.Diagnostics.Debug.Print(sender != null ? sender.ToString() : "");
    }

    #endregion
  }

  public class TypeNameSerializationBinder : SerializationBinder
  {
    public string TypeFormat { get; private set; }

    public TypeNameSerializationBinder(string typeFormat)
    {
      TypeFormat = typeFormat;
    }

    public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
    {
      assemblyName = null;
      typeName = serializedType.Name;
    }

    public override Type BindToType(string assemblyName, string typeName)
    {
      string resolvedTypeName = string.Format(TypeFormat, typeName);

      return Type.GetType(resolvedTypeName, true);
    }
  }
}
