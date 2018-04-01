using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading.Tasks;
using System.ComponentModel;

namespace TMP.Work.Emcos.Controls
{
    using TMPApplication;
    using TMP.Work.Emcos;

    /// <summary>
    /// Interaction logic for APTree.xaml
    /// </summary>
    public partial class APTree : UserControl, INotifyPropertyChanged, IStateObject
    {
        private Model.GRTreeNode _groupAndPoints = new Model.GRTreeNode();
        public APTree()
        {
            InitializeComponent();

            InitPoints();
            aptree.Data.AddRootItem(_groupAndPoints);

            aptree.Data.BeforeNodeExpanded += aptree_BeforeNodeExpanded;
            paramstree.Data.BeforeNodeExpanded += paramstree_BeforeNodeExpanded;          
        }

        public string GetListOfGroupsAndPointsAsString
        {
            get
            {
                var list = aptree.SelectedItems;
                if ((list == null) || (list.Count == 0)) return null;
                var sb = new StringBuilder(list.Count);
                int index = 0;
                foreach (var item in list)
                {
                    var node = item as Model.GRTreeNode;

                    sb.AppendFormat("T1_ID_{0}={1}&", index, node.Id);
                    sb.AppendFormat("T1_TYPE_{0}={1}&", index, node.Type);
                    sb.AppendFormat("T1_NAME_{0}={1} [{2}]&", index, node.Name, node.Code);
                    sb.AppendFormat("T1_POINT_ID_{0}={1}&", index, node.Id);
                    sb.AppendFormat("T1_POINT_NAME_{0}={1}&", index, node.Name);
                    sb.AppendFormat("T1_POINT_CODE_{0}={1}&", index, node.Code);                   
                    sb.AppendFormat("T1_POINT_ENABLED_{0}=1&", index);
                    sb.AppendFormat("T1_POINT_ENABLED_TXT_{0}=Да&", index);
                    sb.AppendFormat("T1_POINT_COMMERCIAL_{0}=Да&", index);
                    sb.AppendFormat("T1_POINT_INTERNAL_{0}=Да&", index);
                    sb.AppendFormat("T1_POINT_AUTO_READ_ENABLED_{0}=Да&", index);
                    sb.AppendFormat("T1_POINT_TYPE_NAME_{0}={1}&", index, node.Type_Name);
                    sb.AppendFormat("T1_POINT_TYPE_CODE_{0}={1}&", index, node.Type_Code);
                    sb.AppendFormat("T1_MOU_BT_{0}=&", index);
                    sb.AppendFormat("T1_MOU_ET_{0}=&", index);
                    sb.AppendFormat("T1_METER_NUMBER_{0}=&", index);
                    sb.AppendFormat("T1_METER_TYPE_NAME_{0}=&", index);
                    sb.AppendFormat("T1_GRP_BT_{0}=&", index);
                    sb.AppendFormat("T1_GRP_DESC_{0}=&", index);
                    sb.AppendFormat("T1_ECP_NAME_{0}={1}&", index, node.Ecp_Name);

                    if (node.Type == "GROUP")
                    {
                        ;
                    }
                    else
                        if (node.Type == "POINT")
                    {
                        ;
                    }
                    else
                        return null;
                    index++;
                }
                sb.AppendFormat("T1_COUNT={0}&", index);
                return sb.ToString();
            }
        }
        public string GetListOfMeasurementsAsString
        {
            get
            {
                var list = paramstree.SelectedItems;
                if ((list == null) || (list.Count == 0)) return null;
                var sb = new StringBuilder(list.Count);
                int index = 0;
                foreach (Model.Param item in list)
                {                    
                    switch (item.Type)
                    {
                        case "MS_TYPE":
                            var ms = item as Model.MS_Param;
                            sb.AppendFormat("T2_TYPE_{0}={1}&", index, ms.Type);
                            sb.AppendFormat("T2_NAME_{0}={1}&", index, ms.Name);
                            sb.AppendFormat("T2_MS_TYPE_ID_{0}={1}&", index, ms.Id);
                            sb.AppendFormat("T2_MS_TYPE_NAME_{0}={1}&", index, ms.Name);
                            break;
                        case "AGGS_TYPE":
                            var aggs = item as Model.AGGS_Param;
                            sb.AppendFormat("T2_TYPE_{0}={1}&", index, aggs.Type);
                            sb.AppendFormat("T2_NAME_{0}={1}&", index, aggs.Name);
                            sb.AppendFormat("T2_AGGS_TYPE_NAME_{0}={1}&", index, aggs.Name);
                            sb.AppendFormat("T2_AGGS_TYPE_ID_{0}={1}&", index, aggs.Id);
                            sb.AppendFormat("T2_MS_TYPE_ID_{0}=1&", index);
                            break;
                        case "MSF":
                            var msf = item as Model.MSF_Param;
                            sb.AppendFormat("T2_TYPE_{0}={1}&", index, msf.Type);
                            sb.AppendFormat("T2_NAME_{0}={1}&", index, msf.Name);
                            sb.AppendFormat("T2_MSF_ID_{0}={1}&", index, msf.Id);
                            sb.AppendFormat("T2_MSF_NAME_{0}={1}&", index, msf.Name);
                            sb.AppendFormat("T2_AGGS_TYPE_ID_{0}={1}&", index, msf.AGGS.Id);
                            sb.AppendFormat("T2_MS_TYPE_ID_{0}=1&", index);
                            break;
                        case "ML":
                            var ml = item as Model.ML_Param;
                            sb.AppendFormat("T2_TYPE_{0}={1}&", index, ml.Type);
                            sb.AppendFormat("T2_NAME_{0}={1}&", index, ml.Name);
                            sb.AppendFormat("T2_ML_ID_{0}={1}&", index, ml.Id);
                            sb.AppendFormat("T2_ML_NAME_{0}={1}&", index, ml.Name);

                            sb.AppendFormat("T2_MS_ID_{0}={1}&", index, ml.MS.Id);
                            sb.AppendFormat("T2_MS_CODE_{0}={1}&", index, ml.MS.Code);
                            sb.AppendFormat("T2_MS_NAME_{0}={1}&", index, ml.MS.Name);
                            sb.AppendFormat("T2_MS_TYPE_ID_{0}={1}&", index, ml.MS.Type_Id);
                            sb.AppendFormat("T2_MS_TYPE_NAME_{0}={1}&", index, ml.MS.Type_Name);

                            sb.AppendFormat("T2_DIR_ID_{0}={1}&", index, ml.DIR.Id);
                            sb.AppendFormat("T2_DIR_CODE_{0}={1}&", index, ml.DIR.Code);
                            sb.AppendFormat("T2_DIR_NAME_{0}={1}&", index, ml.DIR.Name);

                            sb.AppendFormat("T2_AGGS_ID_{0}={1}&", index, ml.AGGS.Id);
                            sb.AppendFormat("T2_AGGS_NAME_{0}={1}&", index, ml.AGGS.Name);
                            sb.AppendFormat("T2_AGGS_VALUE_{0}={1}&", index, ml.AGGS.Value);
                            sb.AppendFormat("T2_AGGS_PER_ID_{0}={1}&", index, ml.AGGS.Per_Id);
                            sb.AppendFormat("T2_AGGS_PER_CODE_{0}={1}&", index, ml.AGGS.Per_Code);
                            sb.AppendFormat("T2_AGGS_PER_NAME_{0}={1}&", index, ml.AGGS.Per_Name);

                            sb.AppendFormat("T2_AGGF_ID_{0}={1}&", index, ml.AGGF.Id);
                            sb.AppendFormat("T2_AGGF_NAME_{0}={1}&", index, ml.AGGF.Name);

                            sb.AppendFormat("T2_TFF_ID_{0}={1}&", index, ml.TFF.Id);
                            sb.AppendFormat("T2_TFF_NAME_{0}={1}&", index, ml.TFF.Name);

                            sb.AppendFormat("T2_EU_ID_{0}={1}&", index, ml.EU.Id);
                            sb.AppendFormat("T2_EU_CODE_{0}={1}&", index, ml.EU.Code);
                            sb.AppendFormat("T2_EU_NAME_{0}={1}&", index, ml.EU.Name);

                            sb.AppendFormat("T2_MD_ID_{0}={1}&", index, ml.MD.Id);
                            sb.AppendFormat("T2_MD_CODE_{0}={1}&", index, ml.MD.Code);
                            sb.AppendFormat("T2_MD_NAME_{0}={1}&", index, ml.MD.Name);                            
                            sb.AppendFormat("T2_MD_INT_VALUE_{0}={1}&", index, ml.MD.Int_Value);
                            sb.AppendFormat("T2_MD_PER_ID_{0}={1}&", index, ml.MD.Per_Id);
                            sb.AppendFormat("T2_MD_PER_CODE_{0}={1}&", index, ml.MD.Per_Code);
                            sb.AppendFormat("T2_MD_PER_NAME_{0}={1}&", index, ml.MD.Per_Name);

                            sb.AppendFormat("T2_OBIS_{0}={1}&", index, ml.OBIS);
                            break;
                    }
                    index++;
                }
                sb.AppendFormat("T2_COUNT={0}&", index);
                return sb.ToString();
            }
        }

        private void InitPoints()
        {
            _groupAndPoints = new Model.GRTreeNode
            {
                Id = "52",
                Code = "1",
                Name = "Белэнерго",
                Type_Id = 7,
                Type_Name = "Белэнерго",
                Type_Code = "CONCERN",
                Parent = "1",
                HasChilds = 1,
                Type = "GROUP",
                IsExpanded = true,
                Children = new ObservableCollection<VTreeView.ITreeNode>
                    {
                        new Model.GRTreeNode
                        {
                            Id = "53",
                            Code = "14",
                            Name = "РУП Гродноэнерго",
                            Type_Id = 3,
                            Type_Name = "Регион",
                            Type_Code = "REGION",
                            Parent = "52",
                            HasChilds = 1,
                            Type = "GROUP",
                            IsExpanded = true,
                            Children = new ObservableCollection<VTreeView.ITreeNode>
                            {
                                new Model.GRTreeNode
                                {
                                    Id = "55",
                                    Code = "14.Генерация РУП",
                                    Name = "Генерация РУП",
                                    Type_Id = 9,
                                    Type_Name = "Генерация",
                                    Type_Code = "GENERATION",
                                    Parent = "53",
                                    HasChilds = 1,
                                    Type = "GROUP",
                                },
                                new Model.GRTreeNode
                                {
                                    Id = "57",
                                    Code = "145.Промпредприятия РУП",
                                    Name = "Промышленные предприятия РУП",
                                    Type_Id = 11,
                                    Type_Name = "Промпредпрятия",
                                    Type_Code = "ENTERPRISE",
                                    Parent = "53",
                                    HasChilds = 1,
                                    Type = "GROUP"
                                },
                                new Model.GRTreeNode
                                {
                                    Id = "54",
                                    Code = "141",
                                    Name = "Гродненские ЭС",
                                    Type_Id = 12,
                                    Type_Name = "ФЭС",
                                    Type_Code = "FES",
                                    Parent = "53",
                                    HasChilds = 1,
                                    Type = "GROUP"
                                },
                                new Model.GRTreeNode
                                {
                                    Id = "58",
                                    Code = "140",
                                    Name = "Волковыские ЭС",
                                    Type_Id = 12,
                                    Type_Name = "ФЭС",
                                    Type_Code = "FES",
                                    Parent = "53",
                                    HasChilds = 1,
                                    Type = "GROUP"
                                },
                                new Model.GRTreeNode
                                {
                                    Id = "59",
                                    Code = "142",
                                    Name = "Лидские ЭС",
                                    Type_Id = 12,
                                    Type_Name = "ФЭС",
                                    Type_Code = "FES",
                                    Parent = "53",
                                    HasChilds = 1,
                                    Type = "GROUP"
                                },
                                new Model.GRTreeNode
                                {
                                    Id = "60",
                                    Code = "143",
                                    Name = "Ошмянские ЭС",
                                    Type_Id = 12,
                                    Type_Name = "ФЭС",
                                    Type_Code = "FES",
                                    Parent = "53",
                                    HasChilds = 1,
                                    Type = "GROUP",
                                    IsExpanded = true,
                                    Children = new ObservableCollection<VTreeView.ITreeNode>
                                    {
                                        new Model.GRTreeNode
                                        {
                                            Id = "2422",
                                            Code = "143.Генерация ОЭС",
                                            Name = "Генерация ОЭС",
                                            Type_Id = 9,
                                            Type_Name = "Генерация",
                                            Type_Code = "GENERATION",
                                            Parent = "60",
                                            HasChilds = 1,
                                            Type = "GROUP"
                                        },
                                        new Model.GRTreeNode
                                        {
                                            Id = "2423",
                                            Code = "143.МежФЭСовские перетоки ОЭС",
                                            Name = "МежФЭСовские перетоки ОЭС",
                                            Type_Id = 27,
                                            Type_Name = "Сальдо",
                                            Type_Code = "Saldo",
                                            Parent = "60",
                                            HasChilds = 1,
                                            Type = "GROUP"
                                        },
                                        new Model.GRTreeNode
                                        {
                                            Id = "2713",
                                            Code = "143.Поступление в сеть 6-10 кВ ОЭС",
                                            Name = "Поступление в сеть 6-10 кВ ОЭС",
                                            Type_Id = 12,
                                            Type_Name = "ФЭС",
                                            Type_Code = "FES",
                                            Parent = "60",
                                            HasChilds = 1,
                                            Type = "GROUP"
                                        },
                                        new Model.GRTreeNode
                                        {
                                            Id = "2588",
                                            Code = "143.Собственные нужды ОЭС",
                                            Name = "Собственные нужды ОЭС",
                                            Type_Id = 12,
                                            Type_Name = "ФЭС",
                                            Type_Code = "FES",
                                            Parent = "60",
                                            HasChilds = 1,
                                            Type = "GROUP"
                                        },
                                        new Model.GRTreeNode
                                        {
                                            Id = "317",
                                            Code = "1431",
                                            Name = "Ивьевский РЭС",
                                            Type_Id = 13,
                                            Type_Name = "РЭС",
                                            Type_Code = "RES",
                                            Parent = "60",
                                            HasChilds = 1,
                                            Type = "GROUP"
                                        },
                                        new Model.GRTreeNode
                                        {
                                            Id = "255",
                                            Code = "1432",
                                            Name = "Сморгонский РЭС",
                                            Type_Id = 13,
                                            Type_Name = "РЭС",
                                            Type_Code = "RES",
                                            Parent = "60",
                                            HasChilds = 1,
                                            Type = "GROUP"
                                        },
                                        new Model.GRTreeNode
                                        {
                                            Id = "294",
                                            Code = "1433",
                                            Name = "Ошмянский РЭС",
                                            Type_Id = 13,
                                            Type_Name = "РЭС",
                                            Type_Code = "RES",
                                            Parent = "60",
                                            HasChilds = 1,
                                            Type = "GROUP"
                                        },
                                        new Model.GRTreeNode
                                        {
                                            Id = "281",
                                            Code = "1434",
                                            Name = "Островецкий РЭС",
                                            Type_Id = 13,
                                            Type_Name = "РЭС",
                                            Type_Code = "RES",
                                            Parent = "60",
                                            HasChilds = 1,
                                            Type = "GROUP"
                                        },
                                        new Model.GRTreeNode
                                        {
                                            Id = "2752",
                                            Code = "1453.Промпредприятия ОЭС",
                                            Name = "Промышленные предприятия ОЭС",
                                            Type_Id = 11,
                                            Type_Name = "Промпредпрятия",
                                            Type_Code = "ENTERPRISE",
                                            Parent = "60",
                                            HasChilds = 1,
                                            Type = "GROUP"
                                        }
                                    }
                                }
                            }
                        }
                    }
            };

            // установка объекта-родителя
            Setparent(_groupAndPoints, null);
        }

        private void Setparent(VTreeView.ITreeNode node, VTreeView.ITreeNode parent)
        {
            node.SetParent(parent);
            if (node.Children == null)
                return;
            foreach (var item in node.Children)
            {
                Setparent(item, node);
            }
        }

        private void aptree_BeforeNodeExpanded(object sender, VTreeView.NodeExpandEventArgs e)
        {
            if (e.Node == null)
                return;
            var node = e.Node as Model.GRTreeNode;
            if (node == null)
                return;
            if (node.Children == null)
            {
                if (node.HasChildren)
                {
                    node.State = VTreeView.TreeNodeState.PrepareChildren;
                    Func<string, bool> post = (data) =>
                    {
                        if (String.IsNullOrWhiteSpace(data)) return false;
                        var list = AnswerParser.ArchivesPoint(data);

                        // установка объекта-родителя
                        foreach (var item in list)
                        {
                            item.SetParent(node);
                        }

                        node.Children = list;
                        // сообщаем, что всё готово
                        DispatcherExtensions.InUi(() => node.State = VTreeView.TreeNodeState.ChildrenPrepared);
                        return true;
                    };
                    Utils.GetData(this, TMP.Work.Emcos.EmcosSiteWrapper.Instance.GetAPointsAsync, node.Id, post);
                }
            }
        }
        /// <summary>
        /// Получение направлений для выбранного измерения
        /// </summary>
        /// <param name="e"></param>
        private void paramstree_BeforeNodeExpanded(object sender, VTreeView.NodeExpandEventArgs e)
        {
            if (e.Node == null)
                return;
            var node = e.Node;

            var param = e.Obj as Model.IParam;
            if (param == null)
                return;

            if (param.Children == null)
            {
                if (param.HasChildren)
                {
                    node.State = VTreeView.TreeNodeState.PrepareChildren;
                    // получаем список идентификаторов выделенных групп и точек
                    if ((aptree == null) || (aptree.SelectedItems == null) || (aptree.SelectedItems.Count == 0)) return;
                    var ids = GetSelectedGroupsAndPointsID(aptree.SelectedItems);

                    // цепочка ИД измерений MS_TYPE -> AGGS_TYPE -> MSF -> DIR
                    var callerUID = string.Empty;
                    var np = node;
                    if (np.ParentNode != null)
                    {
                        while (np != null)
                        {
                            callerUID = "/" + param.Type + "_ID_" + param.Id + callerUID;
                            np = np.ParentNode;
                        }
                    }
                    callerUID = "callerUID=MS_TYPE_ID_1" + callerUID + "&";                    

                    // тип блока и параметр
                    var dataBlock = String.Empty; ;
                    var send_param = String.Empty; ;
                    switch (param.Type)
                    {
                        case "MS_TYPE":
                            dataBlock = "AGGS_TYPE";
                            send_param = "MS_TYPE_ID=1&";
                            break;
                        case "AGGS_TYPE":
                            dataBlock = "MSF";
                            send_param = String.Format("MS_TYPE_ID=1&AGGS_TYPE_ID={0}&", param.Id);
                            break;
                        case "MSF":
                            dataBlock = "DIR";
                            /// ?
                            send_param = String.Format("MSF_ID={0}&AGGS_TYPE_ID={1}&", param.Id, node.ParentNode.Id);
                            break;
                    }
                    var senddata = String.Format("{0}scope=archives&GR_ID={1}&POINT_ID={2}&DS_ID=&applyFilter=true&{3}dataBlock={4}",
                        callerUID,
                        ids[0],
                        ids[1],
                        send_param,
                        dataBlock);
                    //string data = "&MSF_ID_0=14&MSF_NAME_0=А энергия&AGGS_TYPE_ID_0=3&MS_TYPE_ID_0=1&TYPE_0=MSF&MSF_ID_1=15&MSF_NAME_1=R энергия&AGGS_TYPE_ID_1=3&MS_TYPE_ID_1=1&TYPE_1=MSF&result=0&recordCount=2";

                    Func<string, bool> post = (data) =>
                    {
                        if (String.IsNullOrWhiteSpace(data)) return false;

                        var list = Utils.Params(data);

                        if (node == null)
                            DispatcherExtensions.InUi(() =>
                            {
                                paramstree.Data.ClearAll();
                                //paramstree.Data.AddRootItems(list);
                            });
                        else
                        {
                            // установка объекта-родителя
                            foreach (var item in list)
                            {
                                //item.SetParent(node);
                            }

                            //node.Children = list;
                            // сообщаем, что всё готово
                            DispatcherExtensions.InUi(() => node.State = VTreeView.TreeNodeState.ChildrenPrepared);                            
                        }
                        return true;
                    };
                    Utils.GetData(this, EmcosSiteWrapper.Instance.GetParamsAsync, senddata, post);
                }
            }
        }
        /// <summary>
        /// Получение доступных измерений для выбранных групп и точек
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void aptree_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // получаем список идендификаторов выделенных групп и точек
            if ((aptree == null) || (aptree.SelectedItems == null) || (aptree.SelectedItems.Count == 0)) return;
            var ids = GetSelectedGroupsAndPointsID(aptree.SelectedItems);

            var senddata = String.Format("refresh=true&scope=archives&GR_ID={0}&POINT_ID={1}&DS_ID=&applyFilter=true&dataBlock=MS_TYPE",
                ids[0],
                ids[1]);
            Func<string, bool> post = (data) =>
            {
                if (String.IsNullOrWhiteSpace(data)) return false;

                var list = Utils.Params(data);
                //ICollection<VTreeView.ITreeNode> list = AnswerParser.Params(data);

                Dispatcher.BeginInvoke(new Action(() =>
                    {
                        paramstree.Data.ClearAll();
                        //paramstree.Data.AddRootItems(list);
                    }));                
                return true;
            };
            Utils.GetData(this, EmcosSiteWrapper.Instance.GetParamsAsync, senddata, post);
        }

        private string[] GetSelectedGroupsAndPointsID(System.Collections.IList list)
        {
            if (list == null) return null;

            var sbGroups = new StringBuilder(list.Count);
            var sbPoints = new StringBuilder(list.Count);
            foreach (var item in list)
            {
                var element = item as Model.GRTreeNode;
                if (element.Type == "GROUP")
                    sbGroups.AppendFormat("{0},", element.Id);
                else
                    sbPoints.AppendFormat("{0},", element.Id);
            }
            var idsGroups = sbGroups.Length == 0 ? String.Empty : sbGroups.Remove(sbGroups.Length - 1, 1).ToString();
            var idsPoints = sbPoints.Length == 0 ? String.Empty : sbPoints.Remove(sbPoints.Length - 1, 1).ToString();

            return new string[2] { idsGroups, idsPoints };
        }      

        //****************************************************************
        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string propertyName)
        {
            var e = PropertyChanged;
            if (e != null)
                e(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region IStateObject implementation
        private State _state = State.Idle;
        public State State
        {
            get { return _state; }
            set { _state = value; RaisePropertyChanged("State"); }
        }
        public int Progress { get { return 0; } set { } }
        public string Log { get; set; }
        #endregion      
    }
    public enum APStates
    {
        Ready,
        WaitData
    }
}