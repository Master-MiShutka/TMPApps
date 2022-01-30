namespace TMP.UI.Controls.WPF.Reporting.MatrixGrid
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public class MatrixWithSelector : Matrix
    {
        private IEnumerable parameters;
        private object selectedParameter;

        public MatrixWithSelector()
        {
            this.Parameters = new List<string>();
        }

        public IEnumerable Parameters { get => this.parameters; set => this.SetProperty(ref this.parameters, value); }

        public object SelectedParameter
        {
            get
            {
                return this.selectedParameter;
            }

            set
            {
                if (value == null && this.Parameters != null)
                {
                    value = this.Parameters.Cast<object>().First();
                }

                this.SetProperty(ref this.selectedParameter, value);
                if (this.HasData)
                {
                    this.Build();
                }
            }
        }
    }
}
