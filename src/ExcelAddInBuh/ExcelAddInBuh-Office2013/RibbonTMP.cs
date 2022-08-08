using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Tools.Ribbon;

namespace ExcelAddInBuh_Office2013
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

        public event System.Action SummInWordsToRub;
        public event System.Action SummInWordsToDollar;
        public event System.Action SummInWordsToEvro;

        private void buttonRubli_Click(object sender, RibbonControlEventArgs e)
        {
            if (SummInWordsToRub != null)
                SummInWordsToRub();
        }

        private void buttonDollar_Click(object sender, RibbonControlEventArgs e)
        {
            if (SummInWordsToDollar != null)
                SummInWordsToDollar();
        }

        private void buttonEvro_Click(object sender, RibbonControlEventArgs e)
        {
            if (SummInWordsToEvro != null)
                SummInWordsToEvro();
        }
    }
}
