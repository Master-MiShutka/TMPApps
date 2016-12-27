using System.Windows;

namespace TMP.Work.AmperM.TestApp.View
{
  /// <summary>
  /// Interaction logic for ContentWindow.xaml
  /// </summary>
  public partial class ContentWindow : Window
  {
    public ContentWindow()
    {
      InitializeComponent();

      DataContext = ViewModel = new ContentWindowViewModel();
    }

    public ContentWindowViewModel ViewModel { get; set; }
    public class ContentWindowViewModel : ViewModel.AbstractViewModelWaitable
    {
      
    }
  }
}
