using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Microsoft.Office.Tools.Ribbon;

namespace ExcelAddInBuh
{
    public partial class RibbonTMP
    {
        private double? getValue(RibbonComponent obj)
        {
            double value = 0d;
            if (obj == null) return null;
            if (obj.Tag == null) return null;
            string s = obj.Tag.ToString();
            if (String.IsNullOrWhiteSpace(s))
                return null;
            if (Double.TryParse(s, out value))
                return value;
            else
                return null;
        }
        private void buttonDivide_Click(object sender, RibbonControlEventArgs e)
        {
            if (DivideClicked != null)
            {
                RibbonButton b = sender as RibbonButton;
                DivideClicked(getValue(b));
            }

        }

        private void buttonMultiple_Click(object sender, RibbonControlEventArgs e)
        {
            if (MultipleClicked != null)
            {
                RibbonButton b = sender as RibbonButton;
                MultipleClicked(getValue(b));
            }
        }

        public event System.Action<double?> DivideClicked;
        public event System.Action<double?> MultipleClicked;
    }
}
