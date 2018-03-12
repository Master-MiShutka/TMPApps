using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace TMP.UI.Controls.WPF.Reporting.MatrixGrid
{
    /// <summary>
    /// Interaction logic for MatrixGridControl.xaml
    /// </summary>
    [TemplateVisualState(GroupName = "Common", Name = "Ready")]
    [TemplateVisualState(GroupName = "Common", Name = "NoData")]
    [TemplateVisualState(GroupName = "Common", Name = "Building")]
    public partial class MatrixGridControl : ItemsControl
    {
        [Flags]
        public enum State
        {
            /// <summary>
            /// Готово
            /// </summary>
            Ready = 0,
            /// <summary>
            /// Нет данных
            /// </summary>
            NoData = 1,
            /// <summary>
            /// Осуществляется построение матрицы
            /// </summary>
            Building = 2
        }

        State state = State.NoData;

        static MatrixGridControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MatrixGridControl),
                new FrameworkPropertyMetadata(typeof(MatrixGridControl)));
        }


        public MatrixGridControl()
        {
            InitializeComponent();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            UpdateStates(false);
        }

        #region Dependency properties

        public bool ShowRowsTotal
        {
            get { return (bool)GetValue(ShowRowsTotalProperty); }
            set { SetValue(ShowRowsTotalProperty, value); }
        }
        public static readonly DependencyProperty ShowRowsTotalProperty =
            DependencyProperty.Register("ShowRowsTotal", typeof(bool), typeof(MatrixGridControl), new PropertyMetadata(true));

        public bool ShowColumnsTotal
        {
            get { return (bool)GetValue(ShowColumnsTotalProperty); }
            set { SetValue(ShowColumnsTotalProperty, value); }
        }
        public static readonly DependencyProperty ShowColumnsTotalProperty =
            DependencyProperty.Register("ShowColumnsTotal", typeof(bool), typeof(MatrixGridControl), new PropertyMetadata(true));

        public DataTemplate EmptyHeaderTemplate
        {
            get { return (DataTemplate)GetValue(EmptyHeaderTemplateProperty); }
            set { SetValue(EmptyHeaderTemplateProperty, value); }
        }
        public static readonly DependencyProperty EmptyHeaderTemplateProperty =
            DependencyProperty.Register("EmptyHeaderTemplate", typeof(DataTemplate), typeof(MatrixGridControl), new PropertyMetadata(null));

        public IMatrix Matrix
        {
            get { return (IMatrix)GetValue(MatrixProperty); }
            set { SetValue(MatrixProperty, value); }
        }
        public static readonly DependencyProperty MatrixProperty =
            DependencyProperty.Register("Matrix", typeof(IMatrix), typeof(MatrixGridControl), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnMatrixChanged)));

        private static void OnMatrixChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MatrixGridControl target = (MatrixGridControl)d;

            System.ComponentModel.PropertyChangedEventHandler handler = (s, pcea) => { target.state = State.Ready; target.UpdateStates(true); };

            if (e.OldValue != null && e.OldValue is IMatrix oldmatrix)
                oldmatrix.Builded -= handler;
            if (e.NewValue != null && e.NewValue is IMatrix newmatrix)
            {
                newmatrix.ShowRowsTotal = target.ShowRowsTotal;
                newmatrix.ShowColumnsTotal = target.ShowColumnsTotal;

                newmatrix.Builded += handler;
                target.state = State.Building;
            }
            else
            {
                target.state = State.NoData;
            }
            target.UpdateStates(true);
        }


        #endregion

        #region Private methods

        private void UpdateStates(bool useTransitions)
        {
            Dispatcher.BeginInvoke(new Action(delegate
                {
                    switch (state)
                    {
                        case State.Ready:
                            VisualStateManager.GoToState(this, State.Ready.ToString(), false);
                            break;
                        case State.NoData:
                            VisualStateManager.GoToState(this, State.NoData.ToString(), false);
                            break;
                        case State.Building:
                            VisualStateManager.GoToState(this, State.Building.ToString(), false);
                            break;
                        default:
                            VisualStateManager.GoToState(this, state.ToString(), false);
                            break;
                    }
                }));
        }

        #endregion
    }
}
