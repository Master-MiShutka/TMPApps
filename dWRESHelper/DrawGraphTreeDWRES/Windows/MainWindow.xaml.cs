using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

using Model = TMP.DWRES.Objects;
using TMP.DWRES.DB;
using TMP.DWRES.ViewModel;

using TMP.Common.Logger;
using TMPApplication.Logger;
using TMP.Shared.Commands;

namespace TMP.DWRES.GUI
{
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{	
		private MainWindowViewModel vm;

        public MainWindow()
		{
			vm = MainWindowViewModel.Instance;

            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                MessageBoxResult mresult = MessageBox.Show(
                    String.Format("Возникла ошибка!\nОписание:\n{0}.", ex.Message,
                    App.ResourceAssembly.GetName().Name, MessageBoxButton.OK, MessageBoxImage.Error));
            }

            uiScaleSlider.MouseDoubleClick +=
            new MouseButtonEventHandler(RestoreScalingFactor);

            this.Title = String.Format("{0}", vm.MainTitle);

			this.Height = vm.UserPrefs.WindowHeight;
			this.Width = vm.UserPrefs.WindowWidth;
			this.Top = vm.UserPrefs.WindowTop;
			this.Left = vm.UserPrefs.WindowLeft;
			this.WindowState = vm.UserPrefs.WindowState;

            InputBindings.Add(new InputBinding(vm.CommandLoadDBFromFile, new KeyGesture(Key.O, ModifierKeys.Control)));
            InputBindings.Add(new InputBinding(vm.CommandLoadDBFromFile, new KeyGesture(Key.L, ModifierKeys.Control)));

            vm.MainWindow = this;

            #region CommandRelayoutGraph
            vm.CommandRelayoutGraph = new DelegateCommand(() =>
            {
                TMP.DWRES.Graph.FiderGraphLayout fgl = WPFControls.Controls.VisualTreeHelperEx.FindDescendantByType<TMP.DWRES.Graph.FiderGraphLayout>(this);
                fgl.Relayout();
                }
            );
            #endregion 
            #region CommandSelectLayoutAlgorithmType
            vm.CommandSelectLayoutAlgorithmType = new DelegateCommand<Object>((sender) =>
            {
                if (sender == null) return;
                MenuItem mi = sender as MenuItem;
                if (mi == null) return;
                string tag = Convert.ToString(mi.Tag);

                TMP.DWRES.Graph.FiderGraphLayout fgl = WPFControls.Controls.VisualTreeHelperEx.FindDescendantByType<TMP.DWRES.Graph.FiderGraphLayout>(this);

                try
                {
                    fgl.LayoutAlgorithmType = tag;
                }
                catch
                {
                    fgl.LayoutAlgorithmType = "FR";
                }
            }
            );
            #endregion
            #region CommandCopyGraph
            vm.CommandCopyGraph = new DelegateCommand(() =>
            {
                TMP.DWRES.Graph.FiderGraphLayout fgl = WPFControls.Controls.VisualTreeHelperEx.FindDescendantByType<TMP.DWRES.Graph.FiderGraphLayout>(this);

                System.Windows.Media.Imaging.BitmapSource source = PrintHelper.RenderTargetBitmap(fgl, false);

                TMP.Shared.ClipBoardHelper.PasteBitmapSourceToClipboardAsBitmapAndPng(source);
            }
            );
            #endregion
            #region CommandSaveGraph
            vm.CommandSaveGraph = new DelegateCommand(() =>
            {
                string name = String.Format("фидера {0}-{1}", vm.SelectedSubstation.Name, vm.SelectedFider.Name);

                var dlg = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "PNG (*.png) |*.png|JPG (*.jpg)|*.jpg|BMP (*.bmp)|*.bmp",
                    FilterIndex = 0,
                    DefaultExt = "*.png",
                    FileName = String.Format("Схема {0}.png", name),
                    Title = String.Format("Экспорт изображения схемы {0}", name)
                };
                if (dlg.ShowDialog() == true)
                {
                    ImageType imgtype = ImageType.PNG;
                    string ext = System.IO.Path.GetExtension(dlg.FileName);
                    switch (ext)
                    {
                        case ".bmp":
                            imgtype = ImageType.BMP;
                            break;
                        case ".png":
                            imgtype = ImageType.PNG;
                            break;
                        case ".jpg":
                            imgtype = ImageType.JPEG;
                            break;
                        default:
                            imgtype = ImageType.PNG;
                            break;
                    }

                    TMP.DWRES.Graph.FiderGraphLayout fgl = WPFControls.Controls.VisualTreeHelperEx.FindDescendantByType<TMP.DWRES.Graph.FiderGraphLayout>(this);
                    PrintHelper.ExportToImage(fgl, new Uri(dlg.FileName), imgtype);
                }
            }
            );
            #endregion
            #region CommandPrintGraph
            vm.CommandPrintGraph = new DelegateCommand(() =>
            {
                TMP.DWRES.Graph.FiderGraphLayout fgl = WPFControls.Controls.VisualTreeHelperEx.FindDescendantByType<TMP.DWRES.Graph.FiderGraphLayout>(this);

                Size visualSize = new Size(fgl.ActualWidth, fgl.ActualHeight);
                var printControl = TMP.PrintEngine.Utils.PrintControlFactory.Create(visualSize, fgl);
                printControl.ShowPrintPreview();
            }
            );
            #endregion
        }

        private void RestoreScalingFactor(object sender, MouseButtonEventArgs args)
        {
            ((Slider)sender).Value = 1.0;
        }

		#region | Обработка сыбытий |

		private void Window_Closed(object sender, EventArgs e)
		{
			vm.UserPrefs.WindowHeight = this.Height;
			vm.UserPrefs.WindowWidth = this.Width;
			vm.UserPrefs.WindowTop = this.Top;
			vm.UserPrefs.WindowLeft = this.Left;
			vm.UserPrefs.WindowState = this.WindowState;

			vm.UserPrefs.Save();
		}
        #endregion | Обработка сыбытий |
    }
}