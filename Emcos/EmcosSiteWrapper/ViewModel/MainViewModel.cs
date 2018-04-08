using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Text;

namespace TMP.Work.Emcos.ViewModel
{
    using TMP.Work.Emcos.Model;

    public class MainViewModel : PropertyChangedBase
    {
        #region Singleton
        private static MainViewModel instance;
        private MainViewModel()
        {
            Initialize();
        }

        static MainViewModel()
        {
            instance = new MainViewModel();
        }

        public static MainViewModel Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MainViewModel();
                }
                return instance;

            }
        }
        #endregion

        #region | private |        

       

        #endregion

        #region | Public Methods |

        public void Initialize()
        {            
            ML_ID_and_EnergyDirection_dictionary = new Dictionary<decimal, string>();
            ML_ID_and_EnergyDirection_dictionary.Add(49, "plus");
            ML_ID_and_EnergyDirection_dictionary.Add(40, "minus");
            ML_ID_and_EnergyDirection_dictionary.Add(381, "plus");
            ML_ID_and_EnergyDirection_dictionary.Add(382, "minus");
            ML_ID_and_EnergyDirection_dictionary.Add(383, "plus");
            ML_ID_and_EnergyDirection_dictionary.Add(384, "minus");
            ML_ID_and_EnergyDirection_dictionary.Add(385, "plus");
            ML_ID_and_EnergyDirection_dictionary.Add(386, "minus");
            ML_ID_and_EnergyDirection_dictionary.Add(387, "plus");
            ML_ID_and_EnergyDirection_dictionary.Add(388, "minus");
            ML_ID_and_EnergyDirection_dictionary.Add(3489, "minus");
            ML_ID_and_EnergyDirection_dictionary.Add(3490, "minus");
            base.RaisePropertyChanged("ML_ID_and_EnergyDirection_dictionary");

            ML_ID_and_Description_dictionary = new Dictionary<decimal, string>();
            ML_ID_and_Description_dictionary.Add(49, "Средняя P+ мощность за 30 минут");
            ML_ID_and_Description_dictionary.Add(40, "Средняя P-  мощность  за 30  минут");
            ML_ID_and_Description_dictionary.Add(381, "А+ энергия  за  месяц");
            ML_ID_and_Description_dictionary.Add(382, "А-  энергия за  месяц");
            ML_ID_and_Description_dictionary.Add(383, "R+  энергия  за  месяц");
            ML_ID_and_Description_dictionary.Add(384, "R- энергия за  месяц");
            ML_ID_and_Description_dictionary.Add(385, "А+ энергия  за  сутки");
            ML_ID_and_Description_dictionary.Add(386, "А-  энергия за  сутки");
            ML_ID_and_Description_dictionary.Add(387, "R+  энергия  за  сутки");
            ML_ID_and_Description_dictionary.Add(388, "R- энергия за  сутки");
            ML_ID_and_Description_dictionary.Add(3489, "Максимальная  получасовая P-  мощность  за  сутки ");
            ML_ID_and_Description_dictionary.Add(3490, "Максимальная  получасовая P-  мощность  за месяц");
            base.RaisePropertyChanged("ML_ID_and_Description_dictionary");            

        }
        #endregion

        #region | Public Properties |


        

        private EmcosPoint _selectedPoints;
        public EmcosPoint SelectedPoints
        {
            get { return _selectedPoints; }
            set
            {
                _selectedPoints = value;
                SetProp(ref _selectedPoints, value, "SelectedPoints");
            }
        }

        public string SelectedPointsListAsString()
        {
            if (SelectedPoints.Children == null) return String.Empty;
            var result = new StringBuilder();
            foreach (var departament in SelectedPoints.Children)
            {
                foreach (var substation in departament.Children)
                {
                    foreach (var group in substation.Children)
                    {
                        foreach (var subgroup in group.Children)
                        {
                            var eltype = subgroup.ElementType;
                            switch (eltype)
                            {
                                case ElementTypes.VOLTAGE:
                                    break;
                                case ElementTypes.SECTION:
                                    break;
                                case ElementTypes.POWERTRANSFORMER:
                                    result.AppendFormat("{0},", subgroup.Code);
                                    break;
                                case ElementTypes.UNITTRANSFORMER:
                                    result.AppendFormat("{0},", subgroup.Code);
                                    break;
                                case ElementTypes.UNITTRANSFORMERBUS:
                                    result.AppendFormat("{0},", subgroup.Code);
                                    break;
                                case ElementTypes.FIDER:
                                    result.AppendFormat("{0},", subgroup.Code);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
            var data = result.ToString();
            if (String.IsNullOrEmpty(data)) return String.Empty;
            return data.Remove(data.Length - 1, 1);
        }        

        #endregion

        #region | Dictionary |

        public Dictionary<decimal, string> ML_ID_and_EnergyDirection_dictionary { get; private set; }
        public Dictionary<decimal, string> ML_ID_and_Description_dictionary { get; private set; }

        #endregion

        
    }
}
