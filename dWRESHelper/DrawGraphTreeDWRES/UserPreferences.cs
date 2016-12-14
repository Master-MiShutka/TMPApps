using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.DWRES.GUI
{
    public class UserPreferences
    {
        #region Member Variables

        private double windowTop;
        private double windowLeft;
        private double windowHeight;
        private double windowWidth;
        private string fileName = "Ivie.fdb";

        private System.Windows.WindowState windowState;

        #endregion //Member Variables

        #region Public Properties

        public string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }
        public string ShortFileName { get { return System.IO.Path.GetFileName(FileName); } }

        public double WindowTop
        {
            get { return windowTop; }
            set { windowTop = value; }
        }

        public double WindowLeft
        {
            get { return windowLeft; }
            set { windowLeft = value; }
        }

        public double WindowHeight
        {
            get { return windowHeight; }
            set { windowHeight = value; }
        }

        public double WindowWidth
        {
            get { return windowWidth; }
            set { windowWidth = value; }
        }

        public System.Windows.WindowState WindowState
        {
            get { return windowState; }
            set { windowState = value; }
        }

        #endregion //Public Properties

        #region Constructor

        public UserPreferences()
        {
            //Load the settings
            Load();

            //Size it to fit the current screen
            SizeToFit();

            //Move the window at least partially into view
            MoveIntoView();
        }

        #endregion //Constructor

        #region Functions

        /// <summary>
        /// If the saved window dimensions are larger than the current screen shrink the
        /// window to fit.
        /// </summary>
        public void SizeToFit()
        {
            if (windowHeight > System.Windows.SystemParameters.VirtualScreenHeight)
            {
                windowHeight = System.Windows.SystemParameters.VirtualScreenHeight;
            }

            if (windowWidth > System.Windows.SystemParameters.VirtualScreenWidth)
            {
                windowWidth = System.Windows.SystemParameters.VirtualScreenWidth;
            }
        }

        /// <summary>
        /// If the window is more than half off of the screen move it up and to the left 
        /// so half the height and half the width are visible.
        /// </summary>
        public void MoveIntoView()
        {
            if (windowTop + windowHeight / 2 > System.Windows.SystemParameters.VirtualScreenHeight)
            {
                windowTop = System.Windows.SystemParameters.VirtualScreenHeight - windowHeight;
            }

            if (windowLeft + windowWidth / 2 > System.Windows.SystemParameters.VirtualScreenWidth)
            {
                windowLeft = System.Windows.SystemParameters.VirtualScreenWidth - windowWidth;
            }

            if (windowTop < 0)
            {
                windowTop = 0;
            }

            if (windowLeft < 0)
            {
                windowLeft = 0;
            }
        }

        private void Load()
        {
            windowTop = Properties.Settings.Default.WindowTop;
            windowLeft = Properties.Settings.Default.WindowLeft;
            windowHeight = Properties.Settings.Default.WindowHeight;
            windowWidth = Properties.Settings.Default.WindowWidth;
            windowState = Properties.Settings.Default.WindowState;
            fileName = Properties.Settings.Default.FileName;
        }

        public void Save()
        {
            if (windowState != System.Windows.WindowState.Minimized)
            {
                Properties.Settings.Default.WindowTop = windowTop;
                Properties.Settings.Default.WindowLeft = windowLeft;
                Properties.Settings.Default.WindowHeight = windowHeight;
                Properties.Settings.Default.WindowWidth = windowWidth;
                Properties.Settings.Default.WindowState = windowState;
                Properties.Settings.Default.FileName = fileName;

                Properties.Settings.Default.Save();
            }
        }

        #endregion //Functions

    }
}
