namespace TMP.WORK.AramisChetchiki.Extensions
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using System.Windows.Markup;

    public class EnumToItemsSource : MarkupExtension
    {
        private readonly Type type;

        public EnumToItemsSource(Type type)
        {
            this.type = type;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Enum.GetValues(this.type).Cast<object>().Select(e => new
            {
                Value = e,
                Description = GetEnumDescription((Enum)e),
            });
        }

        public static string GetEnumDescription(Enum enumObj)
        {
            FieldInfo fieldInfo = enumObj.GetType().GetField(enumObj.ToString());
            object[] attributes = fieldInfo.GetCustomAttributes(false);
            if (attributes.Length == 0)
            {
                return enumObj.ToString();
            }
            else
            {
                DescriptionAttribute attrib = attributes.OfType<DescriptionAttribute>().FirstOrDefault();
                if (attrib != null)
                {
                    return attrib.Description;
                }
                else
                {
                    return enumObj.ToString();
                }
            }
        }
    }
}
