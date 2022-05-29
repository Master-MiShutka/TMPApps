namespace TMP.UI.Controls.WPF.Reporting.MatrixGrid
{
    using System;
    using System.Collections;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for MatrixGridControl.xaml
    /// </summary>
    [TemplateVisualState(GroupName = "Common", Name = "Ready")]
    [TemplateVisualState(GroupName = "Common", Name = "NoData")]
    [TemplateVisualState(GroupName = "Common", Name = "Building")]
    public class MatrixGridControl : ItemsControl
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
            Building = 2,
        }

        private State state = State.NoData;

        static MatrixGridControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MatrixGridControl), new FrameworkPropertyMetadata(typeof(MatrixGridControl)));
        }

        public MatrixGridControl()
        {
            ;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.UpdateStates(false);
        }

        #region Dependency properties

        public DataTemplate EmptyHeaderTemplate
        {
            get => (DataTemplate)this.GetValue(EmptyHeaderTemplateProperty);
            set => this.SetValue(EmptyHeaderTemplateProperty, value);
        }

        public static readonly DependencyProperty EmptyHeaderTemplateProperty =
            DependencyProperty.Register(nameof(EmptyHeaderTemplate), typeof(DataTemplate), typeof(MatrixGridControl), new PropertyMetadata(null));

        public UIElement LeftContent
        {
            get => (UIElement)this.GetValue(LeftContentProperty);
            set => this.SetValue(LeftContentProperty, value);
        }

        public static readonly DependencyProperty LeftContentProperty =
            DependencyProperty.Register(nameof(LeftContent), typeof(UIElement), typeof(MatrixGridControl), new PropertyMetadata(null));

        public IMatrix Matrix
        {
            get => (IMatrix)this.GetValue(MatrixProperty);
            set => this.SetValue(MatrixProperty, value);
        }

        public static readonly DependencyProperty MatrixProperty =
            DependencyProperty.Register(nameof(Matrix), typeof(IMatrix), typeof(MatrixGridControl), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnMatrixChanged)));

        private static void OnMatrixChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MatrixGridControl target = (MatrixGridControl)d;

            void handlerReady(object s, MatrixBuildedEventArgs mbea)
            {
                target.state = State.Ready;
                target.UpdateStates(true);
            }

            void handlerBuilding(object s, MatrixEventArgs mea)
            {
                target.state = State.Building;
                target.UpdateStates(true);
            }

            if (e.OldValue != null && e.OldValue is IMatrix oldmatrix)
            {
                oldmatrix.Builded -= handlerReady;
                oldmatrix.Building -= handlerBuilding;
            }

            if (e.NewValue != null && e.NewValue is IMatrix newmatrix)
            {
                newmatrix.Builded += handlerReady;
                newmatrix.Building += handlerBuilding;

                if (newmatrix.IsBuilded == false)
                {
                    target.state = State.Building;
                }
                else
                {
                    target.state = State.Ready;
                }
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
            this.Dispatcher.BeginInvoke(new Action(delegate
                {
                    switch (this.state)
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
                            VisualStateManager.GoToState(this, this.state.ToString(), false);
                            break;
                    }
                }));
        }

        #endregion
    }
}
