using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Windows;
using System.Xml;

namespace TMP.Work.AmperM.TestApp.Controls
{
    /// <summary>
    /// Interaction logic for _1CV8SyntaxHighlightTextBox.xaml
    /// </summary>
    public partial class _1CV8SyntaxHighlightTextBox : AvalonTextEditorEx
    {
        public _1CV8SyntaxHighlightTextBox()
        {
            InitializeComponent();
            ShowLineNumbers = true;

            TextArea.Options.EnableHyperlinks = false;
            TextArea.Options.EnableRectangularSelection = true;
            TextArea.Options.ColumnRulerPosition = 0;
            TextArea.SelectionCornerRadius = 3;
            TextArea.IndentationStrategy = new ICSharpCode.AvalonEdit.Indentation.DefaultIndentationStrategy();
            TextArea.Options.HighlightCurrentLine = true;
            TextArea.Options.ShowColumnRuler = true;
#if DEBUG
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this)) return;
#endif

            Uri uri = new Uri("/1CV8Syntax.xshd", UriKind.Relative);
            System.Windows.Resources.StreamResourceInfo info = Application.GetResourceStream(uri);
            IHighlightingDefinition v8Highlighting;
            using (var s = info.Stream)
            {
                using (XmlReader reader = new XmlTextReader(s))
                {
                    v8Highlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.
                        HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
            HighlightingManager.Instance.RegisterHighlighting("1CV8", new string[] { ".v8m" }, v8Highlighting);

            this.SyntaxHighlighting = v8Highlighting;

            ICSharpCode.AvalonEdit.Search.SearchPanel.Install(TextArea);
        }

    private void openFileClick(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.CheckFileExists = true;
            if (ofd.ShowDialog() ?? false)
            {
                Load(ofd.FileName);
            }
        }

        private void saveFileClick(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
            if (sfd.ShowDialog() ?? false)
            {
                Save(sfd.FileName);
            }
        }
    }
}
