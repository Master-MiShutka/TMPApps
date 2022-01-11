namespace UserNotification.ViewModel
{
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// The notification viewmodel organized the backend logic necessary to show the
    /// information content for notifying users about something happening ...
    ///
    /// Based on: http://www.codeproject.com/Articles/499241/Growl-Alike-WPF-Notifications
    /// </summary>
    public class NotificationViewModel : Base.ViewModelBase
    {
        #region fiels
        private int id;
        private string mTitle;
        private string mMessage;
        private BitmapImage mImageIcon;
        private bool mIsTopmost;
        private double mHeight;
        private double mWidth;
        #endregion fiels

        #region constructor

        /// <summary>
        /// Class constructor
        /// </summary>
        public NotificationViewModel()
        {
            this.id = 0;
            this.mTitle = string.Empty;
            this.mMessage = string.Empty;
            this.mImageIcon = null;

            this.mIsTopmost = true;
            this.mHeight = 125;
            this.mWidth = 500;

            // This code is usefule for generating design-time sample code
#if DEBUG
            DependencyObject dep = new DependencyObject();
            if (DesignerProperties.GetIsInDesignMode(dep) == true)
            {
                this.mTitle = "Sample title ...";
                this.mMessage = "This is a sample Message with a rather lomg boring text sample ...";
                this.mImageIcon = null;

                this.mIsTopmost = true;
                this.mHeight = 125;
                this.mWidth = 500;
            }

#endif
        }
        #endregion constructor

        #region properties

        /// <summary>
        /// Get/set notification id.
        /// </summary>
        public int Id
        {
            get => this.id;

            set
            {
                if (this.id == value)
                {
                    return;
                }

                this.id = value;
                this.RaisePropertyChanged(() => this.Id);
            }
        }

        /// <summary>
        /// Get/set title of notification
        /// </summary>
        public string Title
        {
            get => this.mTitle;

            set
            {
                if (this.mTitle == value)
                {
                    return;
                }

                this.mTitle = value;
                this.RaisePropertyChanged(() => this.Title);
            }
        }

        /// <summary>
        /// Get/set message of notification
        /// </summary>
        public string Message
        {
            get => this.mMessage;

            set
            {
                if (this.mMessage == value)
                {
                    return;
                }

                this.mMessage = value;
                this.RaisePropertyChanged(() => this.Message);
            }
        }

        /// <summary>
        /// Get/set ImageUrl to (icon) image shown in the notification
        /// </summary>
        public BitmapImage ImageIcon
        {
            get => this.mImageIcon;

            set
            {
                if (this.mImageIcon == value)
                {
                    return;
                }

                this.mImageIcon = value;
                this.RaisePropertyChanged(() => this.ImageIcon);
            }
        }

        /// <summary>
        /// Get/set property to determine whether notification
        /// is shown in a topmost window or not.
        /// </summary>
        public bool IsTopmost
        {
            get => this.mIsTopmost;

            set
            {
                if (this.mIsTopmost != value)
                {
                    this.mIsTopmost = value;
                    this.RaisePropertyChanged(() => this.IsTopmost);
                }
            }
        }

        /// <summary>
        /// Get/set height of view that displays the notification.
        /// </summary>
        public double ViewHeight
        {
            get => this.mHeight;

            set
            {
                if (this.mHeight != value)
                {
                    this.mHeight = value;
                    this.RaisePropertyChanged(() => this.ViewHeight);
                }
            }
        }

        /// <summary>
        /// Get/set width of view that displays the notification.
        /// </summary>
        public double ViewWidth
        {
            get => this.mWidth;

            set
            {
                if (this.mWidth != value)
                {
                    this.mWidth = value;
                    this.RaisePropertyChanged(() => this.ViewWidth);
                }
            }
        }
        #endregion properties
    }
}