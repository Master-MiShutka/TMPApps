﻿using System.Windows;
using System.Windows.Shapes;

using Interactivity;

namespace TMP.Wpf.CommonControls.Behaviors
{
    public class BindableResourceBehavior : Behavior<Shape>
    {
        public static readonly DependencyProperty ResourceNameProperty = DependencyProperty.Register("ResourceName", typeof(string), typeof(BindableResourceBehavior), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty PropertyProperty = DependencyProperty.Register("Property", typeof(DependencyProperty), typeof(BindableResourceBehavior), new PropertyMetadata(default(DependencyProperty)));

        protected override void OnAttached()
        {
            AssociatedObject.SetResourceReference(Property, ResourceName);
            base.OnAttached();
        }

        public string ResourceName
        {
            get => (string)GetValue(ResourceNameProperty);
            set => SetValue(ResourceNameProperty, value);
        }

        public DependencyProperty Property
        {
            get => (DependencyProperty)GetValue(PropertyProperty);
            set => SetValue(PropertyProperty, value);
        }
    }
}