using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.Shared
{
    class EnumUtilities
    {
    }
}

/*

Method example if we are talking about a ComponentModel.DescriptionAttribute:

public static class EnumUtility
{
    // Might want to return a named type, this is a lazy example (which does work though)
    public static object[] GetValuesAndDescriptions(Type enumType)
    {
        var values = Enum.GetValues(enumType).Cast<object>();
        var valuesAndDescriptions = from value in values
                                    select new
                                        {
                                            Value = value,
                                            Description = value.GetType()
                                                .GetMember(value.ToString())[0]
                                                .GetCustomAttributes(true)
                                                .OfType<DescriptionAttribute>()
                                                .First()
                                                .Description
                                        };
        return valuesAndDescriptions.ToArray();
    }
}
<ObjectDataProvider x:Key="Data" MethodName="GetValuesAndDescriptions"
                    ObjectType="local:EnumUtility">
    <ObjectDataProvider.MethodParameters>
        <x:TypeExtension TypeName="local:TestEnum" />
    </ObjectDataProvider.MethodParameters>
</ObjectDataProvider>
<ListBox ItemsSource="{Binding Source={StaticResource Data}}"
         DisplayMemberPath="Description"
         SelectedValuePath="Value"/>

*/
