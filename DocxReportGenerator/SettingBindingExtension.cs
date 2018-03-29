using System;
using System.Windows.Data;

namespace TMP.Work.DocxReportGenerator
{
    public class SettingBindingExtension : Binding
    {
        public SettingBindingExtension()
        {
            Initialize();
        }

        public SettingBindingExtension(string path)
            : base(path)
        {
            Initialize();
        }

        public SettingBindingExtension(string path, object _default)
            : base(path)
        {
            Initialize();

            int i;
            double d;
            bool b;

            if (_default == null) return;

            if (int.TryParse(_default.ToString(), out i))
            {
                FallbackValue = i;
                TargetNullValue = i;
            }
            else
                if (Double.TryParse(_default.ToString(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out d))
            {
                FallbackValue = d;
                TargetNullValue = d;
            }
            else
                if (Boolean.TryParse(_default.ToString(), out b))
            {
                FallbackValue = b;
                TargetNullValue = b;
            }
            else
            {
                FallbackValue = _default;
                TargetNullValue = _default;
            }
        }

        private void Initialize()
        {
            this.Source = Properties.Settings.Default;
            this.Mode = BindingMode.TwoWay;
        }
    }
}
