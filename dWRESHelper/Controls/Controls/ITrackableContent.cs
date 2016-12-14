using System.Windows;

namespace TMP.WPFControls.Controls
{
    /// <summary>
    /// Interface that represents trackable content object
    /// </summary>
    public interface ITrackableContent
    {
        /// <summary>
        /// Rises when content size changed
        /// </summary>
        event ContentSizeChangedEventHandler ContentSizeChanged;
        /// <summary>
        /// Gets actual content rectangle size
        /// </summary>
        Rect ContentSize { get; }
    }

    public delegate void ContentSizeChangedEventHandler(object sender, ContentSizeChangedEventArgs e);

    public sealed class ContentSizeChangedEventArgs : System.EventArgs
    {
        public Rect OldSize { get; private set; }
        public Rect NewSize { get; private set; }

        public ContentSizeChangedEventArgs(Rect oldSize, Rect newSize)
            : base()
        {
            OldSize = oldSize;
            NewSize = newSize;
        }
    }
}
