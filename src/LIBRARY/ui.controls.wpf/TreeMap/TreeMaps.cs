﻿namespace TMP.UI.WPF.Controls.TreeMap
{
    using System.Windows;
    using System.Windows.Controls;

    public class TreeMaps : ItemsControl
    {
        #region fields

        #endregion

        #region dependency properties

        public static DependencyProperty TreeMapModeProperty
          = DependencyProperty.Register(nameof(TreeMapMode), typeof(TreeMapAlgo), typeof(TreeMaps), new FrameworkPropertyMetadata(TreeMapAlgo.Squarified, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        public static DependencyProperty ValuePropertyNameProperty
          = DependencyProperty.Register(nameof(ValuePropertyName), typeof(string), typeof(TreeMaps), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        public static DependencyProperty MaxDepthProperty
          = DependencyProperty.Register(nameof(MaxDepth), typeof(int), typeof(TreeMaps), new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty MinAreaProperty
          = DependencyProperty.Register(nameof(MinArea), typeof(int), typeof(TreeMaps), new FrameworkPropertyMetadata(64, FrameworkPropertyMetadataOptions.AffectsRender));

        #endregion

        #region ctors

        static TreeMaps()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TreeMaps), new FrameworkPropertyMetadata(typeof(TreeMaps)));
        }

        public TreeMaps()
        {
        }

        #endregion

        #region properties

        public TreeMapAlgo TreeMapMode
        {
            get => (TreeMapAlgo)this.GetValue(TreeMaps.TreeMapModeProperty);
            set => this.SetValue(TreeMaps.TreeMapModeProperty, value);
        }

        public int MaxDepth
        {
            get => (int)this.GetValue(TreeMaps.MaxDepthProperty);
            set => this.SetValue(TreeMaps.MaxDepthProperty, value);
        }

        public int MinArea
        {
            get => (int)this.GetValue(TreeMaps.MinAreaProperty);
            set => this.SetValue(TreeMaps.MinAreaProperty, value);
        }

        public string ValuePropertyName
        {
            get => (string)this.GetValue(TreeMaps.ValuePropertyNameProperty);
            set => this.SetValue(TreeMaps.ValuePropertyNameProperty, value);
        }

        #endregion

        #region protected methods

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TreeMapItem(1, this.MaxDepth, this.MinArea, this.ValuePropertyName);
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is TreeMapItem;
        }

        #endregion

    }

    public enum TreeMapAlgo
    {
        Standard,
        Squarified,
    }
}
