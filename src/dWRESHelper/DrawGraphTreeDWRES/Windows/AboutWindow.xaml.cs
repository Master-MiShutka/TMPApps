using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TMP.DWRES.GUI
{
    /// <summary>
    /// Логика взаимодействия для AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public sealed class AssemblyInfo
    {
        private static AssemblyInfo instance;
        private AssemblyInfo() { }
        public static AssemblyInfo Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AssemblyInfo();
                }
                return instance;
            }
        }
        public string AssemblyTitle
        {
            get
            {
                object[] customAttributes = System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(System.Reflection.AssemblyTitleAttribute), false);
                if (customAttributes.Length > 0)
                {
                    System.Reflection.AssemblyTitleAttribute assemblyTitleAttribute = (System.Reflection.AssemblyTitleAttribute)customAttributes[0];
                    if (assemblyTitleAttribute.Title != "")
                    {
                        return assemblyTitleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
            }
        }
        public string AssemblyVersion
        {
            get
            {
                return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }
        public string AssemblyDescription
        {
            get
            {
                object[] customAttributes = System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(System.Reflection.AssemblyDescriptionAttribute), false);
                if (customAttributes.Length == 0)
                {
                    return "";
                }
                return ((System.Reflection.AssemblyDescriptionAttribute)customAttributes[0]).Description;
            }
        }
        public string AssemblyProduct
        {
            get
            {
                object[] customAttributes = System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(System.Reflection.AssemblyProductAttribute), false);
                if (customAttributes.Length == 0)
                {
                    return "";
                }
                return ((System.Reflection.AssemblyProductAttribute)customAttributes[0]).Product;
            }
        }
        public string AssemblyCopyright
        {
            get
            {
                object[] customAttributes = System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(System.Reflection.AssemblyCopyrightAttribute), false);
                if (customAttributes.Length == 0)
                {
                    return "";
                }
                return ((System.Reflection.AssemblyCopyrightAttribute)customAttributes[0]).Copyright;
            }
        }
        public string AssemblyCompany
        {
            get
            {
                object[] customAttributes = System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(System.Reflection.AssemblyCompanyAttribute), false);
                if (customAttributes.Length == 0)
                {
                    return "";
                }
                return ((System.Reflection.AssemblyCompanyAttribute)customAttributes[0]).Company;
            }
        }
        public DateTime BuildDate
        {
            get
            {
                string buildRevision = this.AssemblyVersion;

                string[] parts = buildRevision.Split('.');
                int build = int.Parse(parts[0]);
                int revision = int.Parse(parts[1]);

                DateTime dateTimeOfBuild = new DateTime(2015, 2, 24)
                                                + new TimeSpan(build, 0, 0, 0)
                                                + TimeSpan.FromSeconds(revision * 2);
                return dateTimeOfBuild;
            }
        }
    }
}
