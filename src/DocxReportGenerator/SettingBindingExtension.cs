namespace TMP.Work.DocxReportGenerator
{
    using System;
    using System.Windows.Data;

    public class SettingBindingExtension : Binding
    {
        public SettingBindingExtension()
        {
            this.Initialize();
        }

        public SettingBindingExtension(string path)
            : base(path)
        {
            this.Initialize();
        }

        public SettingBindingExtension(string path, object _default)
            : base(path)
        {
            this.Initialize();

            int i;
            double d;
            bool b;

            if (_default == null)
            {
                return;
            }

            if (int.TryParse(_default.ToString(), out i))
            {
                this.FallbackValue = i;
                this.TargetNullValue = i;
            }
            else
                if (double.TryParse(_default.ToString(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out d))
            {
                this.FallbackValue = d;
                this.TargetNullValue = d;
            }
            else
                if (bool.TryParse(_default.ToString(), out b))
            {
                this.FallbackValue = b;
                this.TargetNullValue = b;
            }
            else
            {
                this.FallbackValue = _default;
                this.TargetNullValue = _default;
            }
        }

        private void Initialize()
        {
            this.Source = Properties.Settings.Default;
            this.Mode = BindingMode.TwoWay;
        }
    }
}
