﻿using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace TMP.Wpf.CommonControls
{
    public class WindowCommands : ItemsControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ShowSeparatorsProperty
            = DependencyProperty.Register("ShowSeparators",
                                          typeof(bool),
                                          typeof(WindowCommands),
                                          new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender |
                                                                              FrameworkPropertyMetadataOptions.AffectsArrange |
                                                                              FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Gets or sets the value indicating whether to show the separators.
        /// </summary>
        public bool ShowSeparators
        {
            get => (bool)this.GetValue(ShowSeparatorsProperty);
            set => this.SetValue(ShowSeparatorsProperty, value);
        }

        static WindowCommands()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WindowCommands), new FrameworkPropertyMetadata(typeof(WindowCommands)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.ParentWindow = this.TryFindParent<Window>();
        }

        private Window _parentWindow;

        public Window ParentWindow
        {
            get => _parentWindow;
            set
            {
                if (Equals(_parentWindow, value))
                {
                    return;
                }
                _parentWindow = value;
                this.RaisePropertyChanged(nameof(ParentWindow));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged(string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}