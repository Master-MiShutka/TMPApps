namespace TMP.WORK.AramisChetchiki.Extensions
{
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

            if (_default == null)
            {
                return;
            }

            if (int.TryParse(_default.ToString(), out int i))
            {
                this.FallbackValue = i;
                this.TargetNullValue = i;
            }
            else
                if (double.TryParse(_default.ToString(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double d))
            {
                this.FallbackValue = d;
                this.TargetNullValue = d;
            }
            else
                if (bool.TryParse(_default.ToString(), out bool b))
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
            this.Source = AppSettings.Default;
            this.Mode = BindingMode.TwoWay;
        }
    }
}
