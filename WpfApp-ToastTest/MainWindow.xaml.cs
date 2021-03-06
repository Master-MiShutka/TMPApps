﻿using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp_ToastTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.Closing += (s, e) =>
            {
                ShellHelpers.NotificationActivator.Uninitialize();
            };
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            App.ShowToast("Приложение загружено.");
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            App.ShowToast("Test 1.");
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            App.ShowToast("Test 2.");
        }
    }
}
