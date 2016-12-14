using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace TMP.Work.AmperM.TestApp.Controls
{
    public class CompositeTextBox : TextBox
    {
        #region Fields



        #endregion

        #region Constructors

        public CompositeTextBox() : base()
        {
            SetBinding(TextBox.TextProperty, new Binding("Value")
            {
                Source=this,
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });
        }

        #endregion

        #region Properties
        public static readonly DependencyProperty HeaderProperty = DependencyProperty
        .Register("Header",
                typeof(string),
                typeof(CompositeTextBox),
                new FrameworkPropertyMetadata("<нет>"));
        public static readonly DependencyProperty ValueProperty = DependencyProperty
        .Register("Value",
                typeof(string),
                typeof(CompositeTextBox),
                new FrameworkPropertyMetadata("<нет>", ValueChangedCallback));
        public static readonly DependencyProperty ParameterProperty = DependencyProperty
        .Register("Parameter",
                typeof(ViewModel.Funcs.FuncParameter),
                typeof(CompositeTextBox),
                new FrameworkPropertyMetadata(default(ViewModel.Funcs.FuncParameter),
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Inherits,
                    ParameterChangedCallback));
        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }
        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public ViewModel.Funcs.FuncParameter Parameter
        {
            get { return (ViewModel.Funcs.FuncParameter)GetValue(ParameterProperty); }
            set { SetValue(ParameterProperty, value); }
        }

        #endregion

        #region Public Methods



        #endregion

        #region Private Helpers

        static void ValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CompositeTextBox me = d as CompositeTextBox;
            if (me != null && e != null && e.NewValue != null)
            {
                me.Parameter = new ViewModel.Funcs.FuncParameter(me.Header, e.NewValue.ToString());
            }
        }
        static void ParameterChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CompositeTextBox me = d as CompositeTextBox;
            if (me != null && e != null && e.NewValue != null)
            {
                ViewModel.Funcs.FuncParameter newValue = (ViewModel.Funcs.FuncParameter)e.NewValue;
                me.Header = newValue.Name;
                me.SetValue(ValueProperty, newValue.Value);
            }
        }

        #endregion
    }
}