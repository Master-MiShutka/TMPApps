using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using TMP.WORK.AramisChetchiki.Extensions;
using TMP.UI.Controls.WPF;

namespace TMP.WORK.AramisChetchiki.Views
{
    using Model;
    using TMP.Extensions;

    /// <summary>
    /// Элемент управления для отображения сводной информации по коллекции счётчиков
    /// </summary>
    public partial class SummaryInfoView : UserControl
    {
        /// <summary>
        /// список элементов меню для выбора необходимого варианта отображения
        /// </summary>
        private IList<MenuItem> _infoViews;

        public SummaryInfoView()
        {
            InitializeComponent();

            InitViewMenu();

            SetView(InfoViewType.ViewAsDiagram);

            MakeContextMenu();
        }
        /// <summary>
        ///  создание списка меню
        /// </summary>
        private void InitViewMenu()
        {
            _infoViews = EnumHelper<InfoViewType>.GetValues()
                .Select(i =>
                {
                    return new MenuItem()
                    {
                        Header = EnumHelper<InfoViewType>.GetDescriptionValue(i),
                        Tag = "InfoViews",
                        CommandParameter = i,
                        IsCheckable = true,
                        Command = new DelegateCommand<InfoViewType>(view => SetView(view))
                    };
                }).ToList();
            // установка одинаковой группы для элементов в списке
            foreach (var item in _infoViews)
                item.SetValue(MenuItemExtensions.GroupNameProperty, "InfoView");
            // выбор текущего вида
            _infoViews.Where(v => ((InfoViewType)v.CommandParameter) == Properties.Settings.Default.SelectedSummaryView).First().IsChecked = true;
        }
        /// <summary>
        /// установка указанного вида отображения -
        /// применение необходимого шаблона для списка
        /// </summary>
        /// <param name="currentView"></param>
        private void SetView(InfoViewType currentView)
        {
            if (this.itemsControl == null)
                return;

            Properties.Settings.Default.SelectedSummaryView = currentView;

            ControlTemplate template = (ControlTemplate)this.FindResource("ItemsControlBaseTemplate");

            FrameworkElementFactory fef;
            fef = new FrameworkElementFactory(typeof(System.Windows.Controls.StackPanel));
            fef.SetValue(WrapPanel.OrientationProperty, Orientation.Vertical);
            switch (currentView)
            {
                case InfoViewType.ViewAsList:
                    this.itemsControl.ItemsPanel = new ItemsPanelTemplate(fef);
                    if (template != null)
                        this.itemsControl.Template = template;
                    break;
                case InfoViewType.ViewAsTable:
                    this.itemsControl.ItemsPanel = new ItemsPanelTemplate(fef);
                    if (template != null)
                        this.itemsControl.Template = template;
                    break;
                case InfoViewType.ViewAsDiagram:
                    fef = new FrameworkElementFactory(typeof(UniformGrid));

                    template = (ControlTemplate)this.FindResource("ItemsControlWithDiagrams");
                    if (template != null)
                        this.itemsControl.Template = template;
                    this.itemsControl.ItemsPanel = new ItemsPanelTemplate(fef);
                    break;
                default:
                    this.itemsControl.ItemsPanel = new ItemsPanelTemplate(fef);
                    if (template != null)
                        this.itemsControl.Template = template;
                    break;
            }
            this.itemsControl.ItemTemplate = (DataTemplate)App.Current.FindResource(currentView.ToString() + "InfoItemTemplate");
        }
        /// <summary>
        /// создание и установка контекстного меню для списка
        /// </summary>
        private void MakeContextMenu()
        {
            if (this.itemsControl == null)
                throw new NullReferenceException("ItemsControl");
            if (_infoViews == null)
                throw new ArgumentNullException("_infoViews");

            if (this.itemsControl.ContextMenu == null)
            {
                this.itemsControl.ContextMenu = new ContextMenu();
            }
            else
                this.itemsControl.ContextMenu.Items.Add(new Separator());

            MenuItem menuItem = new MenuItem() { Header = "_Вид" };
            foreach (var item in _infoViews)
                menuItem.Items.Add(item);
            this.itemsControl.ContextMenu.Items.Add(menuItem);
        }
    }
}
