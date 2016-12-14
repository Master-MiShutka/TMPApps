using System.Windows;

namespace TMP.WPFControls.Controls
{
    public class AreaSelectedEventArgs : System.EventArgs
    {
        /// <summary>
        /// Rectangle data in coordinates of content object
        /// </summary>
        public Rect Rectangle { get; set; }

        public AreaSelectedEventArgs(Rect rec)
            : base()
        {
            Rectangle = rec;
        }
    }
}
