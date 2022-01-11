namespace TMP.WORK.AramisChetchiki.Controls
{
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for ShowFilters.xaml
    /// </summary>
    public partial class ShowFilters : UserControl
    {
        public ShowFilters()
        {
            var items = ModelHelper.MeterPropertyDescriptors
                .GroupBy(i => string.IsNullOrEmpty(i.GroupName) ? "<нет названия группы>" : i.GroupName)
                .Select(group =>
                    new
                    {
                        GroupHeader = group.Key,
                        GroupFields = group.Select(child =>
                            new
                            {
                                Field = new[] { new { FieldName = child.Name, View = (this.DataContext as ViewModel.BaseDataViewModel<Model.Meter>).View } },
                                FieldDisplayName = child.DisplayName,
                            }),
                    })
                .ToList();

            this.InitializeComponent();

            this.btn.ItemsSource = items;
        }
    }
}
