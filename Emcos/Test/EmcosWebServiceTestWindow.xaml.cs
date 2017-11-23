using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Linq;

using TMPApplication.WpfDialogs.Contracts;

namespace Test
{    

    /// <summary>
    /// Interaction logic for EmcosWebServiceTestWindow.xaml
    /// </summary>
    public partial class EmcosWebServiceTestWindow : TMPApplication.CustomWpfWindow.WindowWithDialogs, INotifyPropertyChanged
    {
        private GRInfoElement _typeNames;
        private GRInfoElement _codes;
        private IList<GRInfoElement> _list1;

        public EmcosWebServiceTestWindow()
        {
            InitializeComponent();

            DataContext = this;

            var s = SystemParameters.VerticalScrollBarWidth;

            ParseGR();
        }

        private void ParseGR()
        {
            XDocument xdoc = XDocument.Load(@"y:\GetGRInfo.xml");

            if (xdoc.Root == null) throw new ArgumentException("Invalid xml format");

            string root = xdoc.Root.Name.LocalName;
            if (string.IsNullOrEmpty(root)) throw new ArgumentException("Invalid xml format");
            try
            {
                if (root == "Envelope")
                {
                    var rows = new List<XElement>();
                    XNamespace root_xmlns = "http://ksmlab.ru/";

                    var orderNode = xdoc.Descendants(root_xmlns + "GetGRInfoResponse");
                    if (orderNode.Count() == 1)
                    {
                        if (orderNode.Elements(root_xmlns + "GetGRInfoResult").Count() == 1)
                        {
                            XNamespace diffgram_xmlns = "urn:schemas-microsoft-com:xml-diffgram-v1";
                            XNamespace msdata_xmlns = "urn:schemas-microsoft-com:xml-msdata";
                            orderNode = xdoc.Descendants(diffgram_xmlns + "diffgram");
                            rows = orderNode.Elements("ROWDATA").Elements().ToList();

                            var list = rows.Select(r => new GRInfoElement {
                                Id = Convert.ToInt32(r.Element("GR_ID").Value),
                                Code = r.Element("GR_CODE").Value,
                                Name = r.Element("GR_NAME").Value,
                                TypeCode = r.Element("GR_TYPE_CODE").Value,
                                TypeName = r.Element("GR_TYPE_NAME").Value                                
                            });

                            var typenames = list.GroupBy(i => i.TypeName).Select(i =>
                            {
                                var result = new GRInfoElement
                                {
                                    Name = i.Key
                                };
                                foreach (var item in i)
                                    result.Children.Add(item);
                                return result;
                            });

                            var l1 = list.Where(i => i.TypeCode == "CONCERN").ToList();
                            foreach (var concern in l1)
                            {
                                var regions = list.Where(i => i.TypeCode == "REGION" && i.Code.StartsWith(concern.Code)).ToList();
                                foreach (var region in regions)
                                {
                                    var feses = list.Where(i => i.TypeCode == "FES" && i.Code.StartsWith(region.Code)).ToList();
                                    foreach (var fes in feses)
                                    {
                                        var reses = list.Where(i => i.TypeCode == "RES" && i.Code.StartsWith(fes.Code)).ToList();
                                        foreach (var res in reses)
                                        {
                                            var substations = list.Where(i => i.TypeCode == "SUBSTATION" && i.Code.StartsWith(res.Code)).ToList();
                                            foreach (var substation in substations)
                                            {
                                                var voltages = list.Where(i => i.TypeCode == "VOLTAGE" && i.Code.StartsWith(substation.Code)).ToList();
                                                foreach (var voltage in voltages)
                                                {
                                                    var sections = list.Where(i => i.TypeCode == "SECTIONBUS" && i.Code.StartsWith(voltage.Code)).ToList();
                                                    voltage.Children.AddRange(sections);
                                                }
                                                substation.Children.AddRange(voltages);
                                            }
                                            res.Children.AddRange(substations);
                                        }
                                        fes.Children.AddRange(reses);
                                    }
                                    region.Children.AddRange(feses);
                                }
                                concern.Children.AddRange(regions);
                            }


                            TypeNames = new GRInfoElement() { Id = 0, Name = "Root" };
                            TypeNames.Children.AddRange(typenames);

                            Codes = new GRInfoElement() { Id = 0, Name = "Root" };
                            Codes.Children.AddRange(l1);

                        }
                    }
                }
                else throw new ArgumentException("Invalid xml format");
            }
            catch (Exception e)
            {
                var s = e.Message;
            }            
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            ;
        }

        public GRInfoElement TypeNames
        {
            get { return _typeNames; }
            set { SetProperty(ref _typeNames, value); }
        }
        public GRInfoElement Codes
        {
            get { return _codes; }
            set { SetProperty(ref _codes, value); }
        }
        public IList<GRInfoElement> List1
        {
            get { return _list1; }
            set { SetProperty(ref _list1, value); }
        }


        #region INotifyPropertyChanged Members

        #region Debugging Aides
        [IgnoreDataMember]
        protected virtual bool ThrowOnInvalidPropertyName { get; private set; }

        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            // Verify that the property name matches a real,
            // public, instance property on this object.
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                string msg = "Invalid property name: " + propertyName;

                if (this.ThrowOnInvalidPropertyName)
                    throw new Exception(msg);
                else
                    Debug.Fail(msg);
            }
        }
        #endregion Debugging Aides
        [field: NonSerializedAttribute()]
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
            this.VerifyPropertyName(propertyName);

            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        #endregion INotifyPropertyChanged Members

        private void WindowWithDialogs_Loaded(object sender, RoutedEventArgs e)
        {
            var s = this.Style;
        }

        private void BtnDialogTest_Info_Click(object sender, RoutedEventArgs e)
        {
<<<<<<< HEAD
            var msg = this.DialogInfo("efefef", "This is caption", MessageBoxImage.Information);            
=======
            var msg = this.DialogInfo("efefef", "This is caption", MessageBoxImage.Information);
>>>>>>> 5913ea3033c1d9573d4f918e829630390915727c
            msg.Show();
        }

        private void BtnDialogTest_InfoYesNo_Click(object sender, RoutedEventArgs e)
        {
            var msg = this.DialogInfo("efefef", "This is caption", MessageBoxImage.Information, TMPApplication.WpfDialogs.DialogMode.YesNo);
            msg.Show();
        }

        private void BtnDialogTest_Warning_Click(object sender, RoutedEventArgs e)
        {
            var msg = this.DialogWarning("efefef", "This is caption");
            msg.Show();
        }

        private void BtnDialogTest_Question_Click(object sender, RoutedEventArgs e)
        {
            var msg = this.DialogQuestion("efefef", "This is caption", TMPApplication.WpfDialogs.DialogMode.OkCancel);
            msg.Show();
        }

        private void BtnDialogTest_Error_Click(object sender, RoutedEventArgs e)
        {
            var msg = this.DialogError("efefef", "This is caption");
            msg.Show();
        }

        private void BtnDialogTest_Exception_Click(object sender, RoutedEventArgs e)
        {
            var msg = this.DialogError(new ArgumentException("First msg", new ArgumentNullException("Nulllll ....")));
            msg.Show();
        }

        private void BtnDialogTest_WaitScreen_Click(object sender, RoutedEventArgs e)
        {
            var msg = this.DialogWaitingScreen("This is wait message", true, TMPApplication.WpfDialogs.DialogMode.Ok);
            msg.Show();
        }

        private void BtnDialogTest_Progress_Click(object sender, RoutedEventArgs e)
        {
            var msg = this.DialogProgress("efefef", null, TMPApplication.WpfDialogs.DialogMode.Ok, false);
            msg.Show();
        }

        private void BtnDialogTest_ProgressIntermediate_Click(object sender, RoutedEventArgs e)
        {
            var msg = this.DialogProgress("efefef", null, TMPApplication.WpfDialogs.DialogMode.Ok, true);
            msg.Show();
        }

        private void BtnDialogTest_Custom_Click(object sender, RoutedEventArgs e)
        {
            UserControl c = new UserControl();
            StackPanel sp = new StackPanel();
            sp.Children.Add(new DatePicker());
            sp.Children.Add(new Slider());
            c.Content = sp;
            var msg = this.DialogCustom(c, TMPApplication.WpfDialogs.DialogMode.Ok);
            msg.Show();
        }
    }

    public class GRInfoElement : ICSharpCode.TreeView.SharpTreeNode
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string TypeCode { get; set; }
        public string TypeName { get; set; }

        public GRInfoElement()
        {
            ;
        }

        public override object Text
        {
            get { return Name; }
        }

        public override object ToolTip
        {
            get
            {
                return String.Format("ID=[{0}], Code=[{1}], TypeCode=[{2}], TypeName=[{3}]", Id, Code, TypeCode, TypeName);
            }
        }
    }
}
