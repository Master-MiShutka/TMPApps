namespace WpfMouseWheel.Windows.Input
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using WpfMouseWheel.Windows.MotionFlow;

    public class MouseWheelFlowDocumentPageViewerScrollClient : MouseWheelClient
    {
        public MouseWheelFlowDocumentPageViewerScrollClient(IMouseWheelController controller)
            : base(controller) { }

        public override ModifierKeys Modifiers => this._modifiersKeys;

        public override bool IsActive(MouseWheelInputEventArgs e)
        {
            return e.Orientation == Orientation.Vertical && base.IsActive(e);
        }

        protected override void OnLoading()
        {
            base.OnLoading();
            var element = this.Controller.Element;
            this._modifiersKeys = MouseWheel.GetVScrollModifiers(element);
            MouseWheel.VScrollModifiersProperty.AddValueChanged(element, this.OnModifierKeysYChanged);
        }

        protected override void OnUnloading()
        {
            var element = this.Controller.Element;
            MouseWheel.VScrollModifiersProperty.RemoveValueChanged(element, this.OnModifierKeysYChanged);
            base.OnUnloading();
        }

        protected override IMouseWheelInputListener CreateBehavior()
        {
            return new MouseWheelFlowDocumentPageViewerScrollBehavior(this);
        }

        public override bool CanMove(IMotionInfo info, object context)
        {
            return info.Direction < 0 ? this.PageViewer.CanGoToPreviousPage : this.PageViewer.CanGoToNextPage;
        }

        public override double Coerce(IMotionInfo info, object context, double delta)
        {
            var p = this.PageViewer;
            if (info.Direction < 0)
            {
                var scrollable = -(p.MasterPageNumber - 1);
                return Math.Max(scrollable, delta);
            }
            else
            {
                var scrollable = p.PageCount - p.MasterPageNumber;
                return Math.Min(scrollable, delta);
            }
        }

        public override void Move(IMotionInfo info, object context, double delta)
        {
            if (info.NativeDirection > 0)
                this.PageViewer.PreviousPage();
            else
                this.PageViewer.NextPage();
        }

        private FlowDocumentPageViewer PageViewer => this.Controller.Element as FlowDocumentPageViewer;

        private void OnModifierKeysYChanged(object sender, EventArgs e)
        {
            this._modifiersKeys = MouseWheel.GetVScrollModifiers(sender as DependencyObject);
        }

        private ModifierKeys _modifiersKeys;
    }
}
