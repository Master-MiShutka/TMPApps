namespace TMP.Shared.ObjectsBuilder
{
    using System.Diagnostics;

    [DebuggerDisplay("Name={FieldName}, Type={FieldType}")]
    public class Field
    {
        public string FieldName;
        public System.Type FieldType;

        public override string ToString()
        {
            return this.FieldName + " - " + this.FieldType.Name;
        }
    }
}
