namespace TMP.ElectricMeterComm
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;

    /// <summary>
    /// Interaction logic for IPAddressControl.xaml
    /// </summary>
    public partial class IPAddressControl : UserControl
    {
        private const string NOT_IP_ADDRESS = "Введенный текст не является IP адресом.";

        #region Constructor

        /// <summary>
        ///  Constructor for the control.
        /// </summary>
        public IPAddressControl()
        {
            this.InitializeComponent();
            this.Loaded += new RoutedEventHandler(this.IPAddressControl_Loaded);

            // txtboxFirstPart.Text = "0";
        }

        #endregion

        #region Private Variables

        private bool focusMoved = false;

        #endregion

        #region Private Methods

        private static void TextboxTextCheck(object sender)
        {
            TextBox txtbox = (TextBox)sender;
            txtbox.Text = GetNumberFromString(txtbox.Text);
            if (!string.IsNullOrWhiteSpace(txtbox.Text))
            {
                if (Convert.ToInt32(txtbox.Text) > 255)
                {
                    txtbox.Text = "255";
                }
                else if (Convert.ToInt32(txtbox.Text) < 0)
                {
                    txtbox.Text = "0";
                }
            }

            txtbox.CaretIndex = txtbox.Text.Length;
        }

        private static string GetNumberFromString(string str)
        {
            StringBuilder numberBuilder = new StringBuilder();
            foreach (char c in str)
            {
                if (char.IsNumber(c))
                {
                    numberBuilder.Append(c);
                }
            }

            return numberBuilder.ToString();
        }

        #endregion

        #region Private Events

        private void IPAddressControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.txtboxFirstPart.Focus();
        }

        private void txtbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                TextboxTextCheck(sender);
            }
            catch (Exception ex)
            {
                throw new Exception(NOT_IP_ADDRESS, ex);
            }
        }

        private void txtboxFirstPart_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.OemPeriod)
                {
                    this.txtboxSecondPart.Focus();
                    this.focusMoved = true;
                }
                else
                {
                    this.focusMoved = false;
                }
            }
            catch
            {
            }
        }

        private void txtboxSecondPart_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.OemPeriod && !this.focusMoved)
                {
                    this.txtboxThridPart.Focus();
                    this.focusMoved = true;
                }
                else
                {
                    this.focusMoved = false;
                }
            }
            catch
            {
            }
        }

        private void txtboxThridPart_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.OemPeriod)
                {
                    this.txtboxFourthPart.Focus();
                }
            }
            catch
            {
            }
        }

        #endregion

        #region Public Properties
        private string text;

        /// <summary>
        /// Gets or Sets the text of the control.
        /// If input text is not of IP type type then throws and argument exception.
        /// </summary>
        public string Text
        {
            get
            {
                this.text = this.txtboxFirstPart.Text + "." + this.txtboxSecondPart.Text + "." + this.txtboxThridPart.Text + "." + this.txtboxFourthPart.Text;
                return this.text;
            }

            set
            {
                try
                {
                    string[] splitValues = value.Split('.');
                    this.txtboxFirstPart.Text = splitValues[0];
                    this.txtboxSecondPart.Text = splitValues[1];
                    this.txtboxThridPart.Text = splitValues[2];
                    this.txtboxFourthPart.Text = splitValues[3];
                    this.text = value;
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(NOT_IP_ADDRESS, ex);
                }
            }
        }
        #endregion

        private void txtboxFirstPart_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                TextBox txtbox = (TextBox)sender;
                if (txtbox.Text.Length == 3)
                {
                    this.txtboxSecondPart.Focus();
                }
            }
            catch
            {
            }
        }

        private void txtboxSecondPart_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                TextBox txtbox = (TextBox)sender;
                if (txtbox.Text.Length == 3)
                {
                    this.txtboxThridPart.Focus();
                }
            }
            catch
            {
            }
        }

        private void txtboxThridPart_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                TextBox txtbox = (TextBox)sender;
                if (txtbox.Text.Length == 3)
                {
                    this.txtboxFourthPart.Focus();
                }
            }
            catch
            {
            }
        }
    }
}
