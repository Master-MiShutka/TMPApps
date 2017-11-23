using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Input;
using System.ComponentModel;
using System.Data;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;

using TMP.UI.Controls.WPF;
using TMP.WORK.AramisChetchiki.Model;
using TMP.Extensions;

namespace TMP.WORK.AramisChetchiki.ViewModel
{
    public class MetrologyViewViewModel : BaseViewModel
    {
        public MetrologyViewViewModel()
        {

        }
        public MetrologyViewViewModel(IList<Meter> meters)
        {
            if (meters == null)
                throw new ArgumentNullException("Meters collection");

            ListOfMeters = meters;
        }

        private IList<Meter> _listOfMeters = null;
        public IList<Meter> ListOfMeters
        {
            get { return _listOfMeters; }
            set { _listOfMeters = value; RaisePropertyChanged("ListOfMeters"); }
        }
    }
}
