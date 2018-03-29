﻿using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
//using TemplateEngine.Docx;

namespace TMP.Work.DocxReportGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainViewModel vm = new MainViewModel();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = vm;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            //File.Delete(tbResultFileName.);
            //File.Copy(tbTemplateFileName.SelectedFile, tbResultFileName.Text);

            /*var valuesToFill = new Content(
                new FieldContent("Report date", DateTime.Now.ToString()));

            using (var outputDocument = new TemplateProcessor(vm.ResultFileName)
                .SetRemoveContentControls(true))
            {
                outputDocument.FillContent(valuesToFill);
                outputDocument.SaveChanges();
            }*/

        }

        private void btnOpenTemplate_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(vm.TemplateFileName);
        }

        private void btnOpenReport_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(vm.ResultFileName);
        }
    }
}