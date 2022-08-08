using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace TMP.Work.Emcos.Model
{
    using Balance;
    using System.Linq;
    using TMP.Shared;

    [DataContract(Namespace = "http://tmp.work.Balance-substations.com")]
    [Magic]
    public class BalanceSession : PropertyChangedBase, IBalanceSession
    {
        SubstationsCollection _substations;
        BalanceSessionInfo _info;

        public BalanceSession()
        {
            Info = new BalanceSessionInfo();
            this.PropertyChanged += BalanceSession_PropertyChanged;
        }

        ~BalanceSession()
        {

        }

        public BalanceSession(IEnumerable<IHierarchicalEmcosPoint> balancePoints, IEnumerable<IHierarchicalEmcosPoint> otherPoints) : this()
        {
            // преобразование колекции
            Balance.SubstationsCollection substations = ModelConverter.PointToBalanceSubstations(new EmcosPoint(balancePoints));
            this.BalancePoints.Create(substations);

            OtherPoints.Clear();
            foreach (IHierarchicalEmcosPoint point in otherPoints)
                OtherPoints.Add(point);
        }

        private void BalanceSession_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "BalancePoints")
                _substations = null;
        }

        private void Info_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Period" || e.PropertyName == "IsLoaded")
                RaisePropertyChanged("Info");
        }

        #region Properties

        /// <summary>
        /// Общая информация
        /// </summary>
        [DataMember]
        [Magic]
        public BalanceSessionInfo Info
        {
            get => _info;
            set
            {
                if (_info != null)
                    _info.PropertyChanged -= Info_PropertyChanged;
                SetProperty(ref _info, value, "Info");
                if (_info != null)
                    _info.PropertyChanged += Info_PropertyChanged;
            }
        }

        /// <summary>
        /// Коллекция элементов для расчёта баланса энергии
        /// </summary>
        [DataMember]
        public HierarchicalEmcosPointCollection BalancePoints { get; internal set; } = new HierarchicalEmcosPointCollection(null);

        /// <summary>
        /// Коллекция прочих элементов
        /// </summary>
        [DataMember]
        public HierarchicalEmcosPointCollection OtherPoints { get; internal set; } = new HierarchicalEmcosPointCollection(null);

        /// <summary>
        /// Словарь пар код группы - баланс активной энергии
        /// </summary>
        [DataMember]
        public Dictionary<int, Balance<ActiveEnergy>> ActiveEnergyBalanceById { get; internal set; } = new Dictionary<int, Balance<ActiveEnergy>>();
        /// <summary>
        /// Словарь пар код группы - баланс реактивной энергии
        /// </summary>
        [DataMember]
        public Dictionary<int, Balance<ReactiveEnergy>> ReactiveEnergyBalanceById { get; internal set; } = new Dictionary<int, Balance<ReactiveEnergy>>();
        /// <summary>
        /// Словарь пар код элемента - активная энергия
        /// </summary>
        [DataMember]
        public Dictionary<int, ActiveEnergy> ActiveEnergyById { get; internal set; } = new Dictionary<int, ActiveEnergy>();
        /// <summary>
        /// Словарь пар код элемента - реактивная энергия
        /// </summary>
        [DataMember]
        public Dictionary<int, ReactiveEnergy> ReactiveEnergyById { get; internal set; } = new Dictionary<int, ReactiveEnergy>();

        /// <summary>
        /// Словарь пар код элемента - описание
        /// </summary>
        [DataMember]
        public Dictionary<int, string> DescriptionsById { get; internal set; } = new Dictionary<int, string>();

        #region Others

        /// <summary>
        /// Список подразделений
        /// </summary>
        [IgnoreDataMember]
        public IList<IHierarchicalEmcosPoint> Departaments
        {
            get
            {
                if (BalancePoints == null)
                    return new List<IHierarchicalEmcosPoint>();
                IList<IHierarchicalEmcosPoint> result = BalancePoints?.Where(p => p.TypeCode == "RES")
                        .OrderBy(p1 => p1.Name)
                        .ToList();
                result.Insert(0, new EmcosPoint { Name = Emcos.Strings.AllReses });
                return new BindableCollection<IHierarchicalEmcosPoint>(result);
            }
        }

        /// <summary>
        /// Список подстанций
        /// </summary>
        [IgnoreDataMember]
        public SubstationsCollection Substations
        {
            get
            {
                if (BalancePoints == null)
                    return new SubstationsCollection();
                if (_substations == null)
                {
                    _substations = new SubstationsCollection(null, BalancePoints?.FlatItemsList
                      .Where(p => p.TypeCode == "SUBSTATION" || p.ElementType == ElementTypes.SUBSTATION)
                      .OrderBy(p1 => p1.Name)
                      );
                }
                return _substations;
            }
        }

        #endregion

        #endregion

        #region Public methods

        public void UpdateValueInDictionary(int elementId, object value, System.Collections.IDictionary dictionary)
        {
            if (dictionary.Contains(elementId))
                dictionary[elementId] = value;
            else
                throw new Exception();
        }

        #endregion
    }
}
