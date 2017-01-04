using System.Diagnostics;

namespace TMP.Shared.ObjectsBuilder
{
    [DebuggerDisplay("Name={FieldName}, Type={FieldType}")]
    public class Field
    {
        public string FieldName;
        public System.Type FieldType;

        public override string ToString()
        {
            return FieldName + " - " + FieldType.Name;
        }
    }
}
