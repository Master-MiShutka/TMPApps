namespace TMP.WORK.AramisChetchiki.Controls
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    /// <summary>
    /// Interaction logic for AramisDbSelector.xaml
    /// </summary>
    public partial class AramisDbSelector : UserControl
    {
        private Cursor oldCursor;

        public AramisDbSelector()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// База данных найдена и выбрана
        /// </summary>
        public bool IsOk
        {
            get => (bool)this.GetValue(IsOkProperty);
            set => this.SetValue(IsOkProperty, value);
        }

        // Using a DependencyProperty as the backing store for IsOk.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsOkProperty =
            DependencyProperty.Register(nameof(IsOk), typeof(bool), typeof(AramisDbSelector), new PropertyMetadata(false));

        /// <summary>
        /// Сообщение, содержащее результат проверки выбранного пути к базе данных
        /// </summary>
        public string DbPathValidationMessage
        {
            get => (string)this.GetValue(DbPathValidationMessageProperty);
            set => this.SetValue(DbPathValidationMessageProperty, value);
        }

        // Using a DependencyProperty as the backing store for DbPathValidationMessage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DbPathValidationMessageProperty =
            DependencyProperty.Register(nameof(DbPathValidationMessage), typeof(string), typeof(AramisDbSelector), new PropertyMetadata(string.Empty));

        /// <summary>
        /// Путь к базе данных
        /// </summary>
        public string DbPath
        {
            get => (string)this.GetValue(DbPathProperty);
            set => this.SetValue(DbPathProperty, value);
        }

        // Using a DependencyProperty as the backing store for DbPath.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DbPathProperty =
            DependencyProperty.Register(nameof(DbPath), typeof(string), typeof(AramisDbSelector), new PropertyMetadata(string.Empty, new PropertyChangedCallback(OnDbPathChanged)));

        private static void OnDbPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AramisDbSelector aramisDbSelector = (AramisDbSelector)d;
            string path = (string)e.NewValue;

            if (string.IsNullOrWhiteSpace(path))
            {
                aramisDbSelector.DbPathValidationMessage = string.Empty;
                aramisDbSelector.IsOk = false;
                return;
            }

            if (System.IO.Directory.Exists(path) == false)
            {
                aramisDbSelector.DbPathValidationMessage = "Указанный путь не существует!";
                aramisDbSelector.IsOk = false;
                return;
            }

            Task.Factory.StartNew(
                () =>
            {
                try
                {
                    if (Repository.Instance.ContainsDataInfoCollectionAramisDbPath(path))
                    {
                        aramisDbSelector.DbPathValidationMessage = "Это подразделение уже добавлено. Загружайте.";
                        aramisDbSelector.IsOk = true;
                    }
                    else
                    {
                        string departamentName = Repository.GetDepartamentName(path);
                        if (string.IsNullOrEmpty(departamentName))
                        {
                            aramisDbSelector.DbPathValidationMessage = "Эта папка не содержит базу данных программы 'Арамис'!";
                            aramisDbSelector.IsOk = false;
                        }
                        else
                        {
                            aramisDbSelector.DbPathValidationMessage = $"OK. Выбрана база данных программы 'Арамис', подразделение '{departamentName}'.";
                            aramisDbSelector.IsOk = true;
                        }
                    }
                }
                catch (System.IO.IOException)
                {
                    aramisDbSelector.DbPathValidationMessage = "Произошла ошибка ввода-вывода. Попробуйте ещё раз.";
                    aramisDbSelector.IsOk = false;
                }
                catch (Exception)
                {
                    aramisDbSelector.DbPathValidationMessage = "Произошла ошибка. Попробуйте ещё раз.";
                    aramisDbSelector.IsOk = false;
                }
            }, CancellationToken.None, TaskCreationOptions.AttachedToParent, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void SelectFolderTextBox_OnStartSelectFolder(object sender, EventArgs e)
        {
            this.oldCursor = this.Cursor;
            this.ForceCursor = true;
            this.Cursor = Cursors.Wait;
        }

        private void SelectFolderTextBox_OnEndSelectFolder(object sender, EventArgs e)
        {
            this.Cursor = this.oldCursor;
        }
    }
}
