using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TMP.Work.Emcos.DataForCalculateNormativ
{
    public class EnergyValues : INotifyPropertyChanged
    {
        private decimal? _auxiliaryAPlus, _auxiliaryAMinus, _aEnergyPlus, _aEnergyMinus, _rEnergyPlus, _rEnergyMinus;

        public decimal? AuxiliaryAPlus
        {
            get { return _auxiliaryAPlus; }
            set { SetProperty(ref _auxiliaryAPlus, value); }
        }
        public decimal? AuxiliaryAMinus
        {
            get { return _auxiliaryAMinus; }
            set { SetProperty(ref _auxiliaryAMinus, value); }
        }
        public decimal? AEnergyPlus
        {
            get { return _aEnergyPlus; }
            set { SetProperty(ref _aEnergyPlus, value); }
        }
        public decimal? AEnergyMinus
        {
            get { return _aEnergyMinus; }
            set { SetProperty(ref _aEnergyMinus, value); }
        }
        public decimal? REnergyPlus
        {
            get { return _rEnergyPlus; }
            set { SetProperty(ref _rEnergyPlus, value); }
        }
        public decimal? REnergyMinus
        {
            get { return _rEnergyMinus; }
            set { SetProperty(ref _rEnergyMinus, value); }
        }

        #region INotifyPropertyChanged Members

        #region Debugging Aides

        protected virtual bool ThrowOnInvalidPropertyName { get; private set; }

        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            // Verify that the property name matches a real,
            // public, instance property on this object.
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                string msg = "Invalid property name: " + propertyName;

                if (this.ThrowOnInvalidPropertyName)
                    throw new Exception(msg);
                else
                    Debug.Fail(msg);
            }
        }
        #endregion Debugging Aides

        public event PropertyChangedEventHandler PropertyChanged;

        public bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return false;
            }

            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.VerifyPropertyName(propertyName);

            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        #endregion INotifyPropertyChanged Members
    }
}
