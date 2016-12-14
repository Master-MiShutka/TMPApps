using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace TMP.Work.Emcos.Controls
{
    using Model;
    /// <summary>
    /// Interaction logic for DateRange.xaml
    /// </summary>
    [TemplatePart(Name = ElementRootPanel, Type = typeof(DockPanel))]
    [TemplatePart(Name = ElementDropDownButton, Type = typeof(ToggleButton))]
    [TemplatePart(Name = ElementSetButton, Type = typeof(Button))]
    [TemplatePart(Name = ElementPopup, Type = typeof(Popup))]
    [TemplatePart(Name = ElementStartDatePicker, Type = typeof(DatePicker))]
    [TemplatePart(Name = ElementEndDatePicker, Type = typeof(DatePicker))]
    public class DateRange : ContentControl
    {
        #region Constants
        private const string ElementRootPanel = "PART_RootPanel";
        private const string ElementDropDownButton = "PART_DropDownButton";
        private const string ElementSetButton = "PART_SetButton";
        private const string ElementPopup = "PART_Popup";
        private const string ElementStartDatePicker = "PART_StartDatePicker";
        private const string ElementEndDatePicker = "PART_EndDatePicker";
        #endregion

        #region Data
        private bool _skipCheck = false;
        private DockPanel _rootPanel;
        private ToggleButton _dropDownButton;
        private Button _setButton;
        private Popup _popup;
        private DatePicker _startDatePicker;
        private DatePicker _endDatePicker;

        #endregion

        #region Public Events

        public static readonly RoutedEvent StartDateChangedEvent = EventManager.RegisterRoutedEvent("StartDateChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DateRange));
        public static readonly RoutedEvent EndDateChangedEvent = EventManager.RegisterRoutedEvent("EndDateChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DateRange));

        public event RoutedEventHandler StartDateChanged
        {
            add { AddHandler(StartDateChangedEvent, value); }
            remove { RemoveHandler(StartDateChangedEvent, value); }
        }
        public event RoutedEventHandler EndDateChanged
        {
            add { AddHandler(EndDateChangedEvent, value); }
            remove { RemoveHandler(EndDateChangedEvent, value); }
        }

        #endregion

        static DateRange()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DateRange), new FrameworkPropertyMetadata(typeof(DateRange)));

            VerticalContentAlignmentProperty.OverrideMetadata(typeof(DateRange), new FrameworkPropertyMetadata(VerticalAlignment.Center));
        }
        public DateRange()
        {
            //DataContext = this;
        }

        public override void OnApplyTemplate()
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                return;

            if (_dropDownButton != null)
            {
                _dropDownButton.Click -= DropDownButton_Click;
            }
            if (_setButton != null)
            {
                _setButton.Click -= SetButton_Click;
            }

            base.OnApplyTemplate();

            _rootPanel = GetTemplateChild(ElementRootPanel) as DockPanel;
            _dropDownButton = GetTemplateChild(ElementDropDownButton) as ToggleButton;
            _setButton = GetTemplateChild(ElementSetButton) as Button;
            _popup = GetTemplateChild(ElementPopup) as Popup;
            _startDatePicker = GetTemplateChild(ElementStartDatePicker) as DatePicker;
            _endDatePicker = GetTemplateChild(ElementEndDatePicker) as DatePicker;
            if (_rootPanel == null || _dropDownButton == null || _setButton == null || _popup == null || _startDatePicker == null || _endDatePicker == null)
            {
                throw new InvalidOperationException(string.Format("You have missed to specify {0}, {1}, {2}, {3}, {4}, {5} in your template", 
                    ElementRootPanel, ElementDropDownButton, ElementSetButton, ElementPopup, ElementStartDatePicker, ElementEndDatePicker));
            }
            _rootPanel.ContextMenu = new ContextMenu();
            _rootPanel.ContextMenu.Items.Add(new MenuItem { Header = "Сегодня", Tag = "today" });
            _rootPanel.ContextMenu.Items.Add(new MenuItem { Header = "Вчера", Tag = "yesterday" });
            _rootPanel.ContextMenu.Items.Add(new MenuItem { Header = "Текущая неделя", Tag = "this week" });
            _rootPanel.ContextMenu.Items.Add(new MenuItem { Header = "Предыдущая неделя", Tag = "prev week" });
            _rootPanel.ContextMenu.Items.Add(new MenuItem { Header = "Предыдущая и текущая неделя", Tag = "prev and this week" });
            _rootPanel.ContextMenu.Items.Add(new MenuItem { Header = "Текущий месяц", Tag = "this month" });
            _rootPanel.ContextMenu.Items.Add(new MenuItem { Header = "Предыдущий месяц", Tag = "prev month" });
            _rootPanel.ContextMenu.Items.Add(new MenuItem { Header = "Предыдущий и текущий месяц", Tag = "prev and this month" });
            foreach (MenuItem item in _rootPanel.ContextMenu.Items)
                item.Click += ContextMenuItem_Click;

            if (_dropDownButton != null)
            {
                _dropDownButton.Click += DropDownButton_Click;
            }
            if (_setButton != null)
            {
                _setButton.Click += SetButton_Click;
            }
            if (_popup != null)
            {
                _popup.AddHandler(PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(PopUp_PreviewMouseLeftButtonDown));
                _popup.Opened += _PopUp_Opened;
            }
        }

        #region Private Methods

        private void PopUp_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var popup = sender as Popup;
            if (popup != null && !popup.StaysOpen)
            {
                ;
            }
        }

        private void _PopUp_Opened(object sender, EventArgs e)
        {
            ;
        }

        private void ContextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (e.Source == null) return;
            var mi = e.Source as MenuItem;
            if (mi == null) return;
            var tag = mi.Tag == null ? null : mi.Tag.ToString();

            _skipCheck = true;

            var today = DateTime.Today;
            var month = new DateTime(today.Year, today.Month, 1);
            switch (tag)
            {
                case "today":
                    Period = new DatePeriod(today, today);
                    break;
                case "yesterday":
                    Period = new DatePeriod(today.AddDays(-1), today.AddDays(-1));
                    break;
                case "this week":
                    int diff = today.DayOfWeek - DayOfWeek.Monday;
                    if (diff < 0)
                    {
                        diff += 7;
                    }
                    Period = new DatePeriod(today.AddDays(-1 * diff).Date, today);
                    break;
                case "prev week":
                    diff = today.DayOfWeek - (DayOfWeek.Monday + 7);
                    if (diff < 0)
                    {
                        diff += 14;
                    }
                    Period = new DatePeriod(today.AddDays(-1 * diff).Date, today.AddDays(-1 * diff).AddDays(6));
                    break;
                case "prev and this week":
                    diff = today.DayOfWeek - (DayOfWeek.Monday + 7);
                    if (diff < 0)
                    {
                        diff += 14;
                    }
                    Period = new DatePeriod(today.AddDays(-1 * diff).Date, today);
                    break;
                case "this month":
                    Period = new DatePeriod(month, today);
                    break;
                case "prev month":
                    Period = new DatePeriod(month.AddMonths(-1), month.AddDays(-1));
                    break;
                case "prev and this month":
                    Period = new DatePeriod(month.AddMonths(-1), today);
                    break;
            }
            _skipCheck = false;
        }


        private void DropDownButton_Click(object sender, RoutedEventArgs e)
        {
            TogglePopup();
        }
        private void SetButton_Click(object sender, RoutedEventArgs e)
        {
            _popup.IsOpen = false;
            Period = new DatePeriod(_startDatePicker.SelectedDate, _endDatePicker.SelectedDate);
            SelectedPeriodAsText = Period.GetFriendlyDateRange();
        }
        private void TogglePopup()
        {
            ;
        }
        private static void OnPeriodChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //if (_skipCheck) return;
            var dr = d as DateRange;
            if (dr == null)
                return;
            SetProperties(dr);
        }

        private static void SetProperties(DateRange dr)
        {
            dr.SelectedPeriodAsText = dr.Period.GetFriendlyDateRange();
            if (dr._startDatePicker != null)
                dr._startDatePicker.SelectedDate = dr.Period.StartDate;
            if (dr._endDatePicker != null)
                dr._endDatePicker.SelectedDate = dr.Period.EndDate;
        }

        #endregion

        #region Properties

        #region LabelTextWrappingProperty dependency property
        public static readonly DependencyProperty LabelTextWrappingProperty =
              DependencyProperty.Register("LabelTextWrapping", typeof(bool), typeof(DateRange), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

        public bool LabelTextWrapping
        {
            get { return (bool)GetValue(LabelTextWrappingProperty); }
            set { SetValue(LabelTextWrappingProperty, value); }
        }
        #endregion
        #region  IconSizeProperty dependency property
        public static readonly DependencyProperty IconSizeProperty =
              DependencyProperty.Register("IconSize", typeof(double), typeof(DateRange), new FrameworkPropertyMetadata(24d, FrameworkPropertyMetadataOptions.AffectsRender));

        public double IconSize
        {
            get { return (double)GetValue(IconSizeProperty); }
            set { SetValue(IconSizeProperty, value); }
        }
        #endregion
        #region PeriodProperty dependency property
        public static readonly DependencyProperty PeriodProperty =
                      DependencyProperty.Register("Period", typeof(DatePeriod), typeof(DateRange), new FrameworkPropertyMetadata(default(DatePeriod), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPeriodChanged, null));

        public DatePeriod Period
        {
            get { return (DatePeriod)GetValue(PeriodProperty); }
            set { SetValue(PeriodProperty, value);  }
        }

        #endregion
           
        #region SelectedPeriodProperty dependency property
        public static readonly DependencyProperty SelectedPeriodAsTextProperty =
              DependencyProperty.Register("SelectedPeriodAsText", typeof(string), typeof(DateRange), new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string SelectedPeriodAsText
        {
            get { return (string)GetValue(SelectedPeriodAsTextProperty); }
            private set { SetValue(SelectedPeriodAsTextProperty, value); }
        }
        #endregion

        #region ChangeButtonTitleProperty dependency property
        public static readonly DependencyProperty ChangeButtonTitleProperty =
              DependencyProperty.Register("ChangeButtonTitle", typeof(string), typeof(DateRange), new FrameworkPropertyMetadata(". . .", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string ChangeButtonTitle
        {
            get { return (string)GetValue(ChangeButtonTitleProperty); }
            set { SetValue(ChangeButtonTitleProperty, value); }
        }
        #endregion

        #endregion

        #region Public Methods
        #endregion
    }    
}
