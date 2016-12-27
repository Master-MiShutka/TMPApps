namespace TMP.Work.AmperM.TestApp.ViewModel.Funcs
{
    [System.Diagnostics.DebuggerDisplay("name={Name}, value={Value}")]
    public struct FuncParameter
    {
        public string Name { get; set; }
        public string Value { get; set;}
        public FuncParameter(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }
        public FuncParameter(FuncParameter old, string value)
        {
            this.Name = old.Name;
            this.Value = value;
        }
    }
}