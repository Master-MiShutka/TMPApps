using System.Windows.Automation.Peers;

namespace TMP.Wpf.Common.MsgBox
{
    class WPFMessageBoxControlAutomationPeer : UIElementAutomationPeer
    {
        public WPFMessageBoxControlAutomationPeer(WPFMessageBoxControl owner) : base(owner)
        {
        }

        protected override string GetClassNameCore()
        {
            return "WPFMessageBoxControl";
        }
    }
}
