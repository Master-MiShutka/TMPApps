using System;
using System.Windows;
using System.Windows.Controls;

namespace TMP.Wpf.Common.MsgBox
{
    [TemplatePart(Name = "PART_YesButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_NoButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_OkButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_CancelButton", Type = typeof(Button))]
    public class WPFMessageBoxControl : Control
    {
        static WPFMessageBoxControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WPFMessageBoxControl), new FrameworkPropertyMetadata(typeof(WPFMessageBoxControl)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            YesButton = base.GetTemplateChild("PART_YesButton") as Button;
            NoButton = base.GetTemplateChild("PART_NoButton") as Button;
            OkButton = base.GetTemplateChild("PART_OkButton") as Button;
            CancelButton = base.GetTemplateChild("PART_CancelButton") as Button;
        }

        internal Button YesButton { get; set; }
        internal Button NoButton { get; set; }
        internal Button OkButton { get; set; }
        internal Button CancelButton { get; set; }

        protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
        {
            return new WPFMessageBoxControlAutomationPeer(this);
        }
    }
}
