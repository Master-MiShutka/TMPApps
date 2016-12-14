using System;
using System.Windows.Data;
using System.Xml.Linq;

namespace TMP.ARMTES
{
    public class SettingBindingExtension : Binding
    {
        public static XDocument XDoc;

        public SettingBindingExtension()
        {
            Initialize();
        }

        public SettingBindingExtension(string path)
            : base(path)
        {
            Initialize();
        }

        public SettingBindingExtension(string path, string _default)
            : base(path)
        {
            Initialize();

            int i;
            double d;

            if (String.IsNullOrEmpty(_default)) return;
            string last = _default.Substring(_default.Length - 1);

            switch (last)
            {
                case "d":
                    if (double.TryParse(_default.Substring(0, _default.Length - 2), out d))
                    {
                        FallbackValue = d;
                        TargetNullValue = d;
                    }
                    break;

                default:
                    {
                        if (int.TryParse(_default, out i))
                        {
                            FallbackValue = i;
                            TargetNullValue = i;
                        }
                        else if (double.TryParse(_default, out d))
                        {
                            FallbackValue = d;
                            TargetNullValue = d;
                        }
                        else
                        {
                            FallbackValue = _default;
                            TargetNullValue = _default;
                        }
                    }
                    break;
            }
        }

        private void Initialize()
        {
            this.Source = TMP.ARMTES.Properties.Settings.Default;
            this.Mode = BindingMode.TwoWay;
        }
    }
}