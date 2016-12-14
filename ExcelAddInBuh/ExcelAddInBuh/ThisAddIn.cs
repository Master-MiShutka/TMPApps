using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using Excel = Microsoft.Office.Interop.Excel;
using Office = Microsoft.Office.Core;
using Microsoft.Office.Tools.Excel;

namespace ExcelAddInBuh
{
    public partial class ThisAddIn
    {
        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
        }

        #region VSTO generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }

        #endregion

        protected override Microsoft.Office.Core.IRibbonExtensibility CreateRibbonExtensibilityObject()
        {
            try
            {
                var r = new RibbonTMP();
                r.DivideClicked += RibbonButtonDivide_Clicked;
                r.MultipleClicked += RibbonButtonMultiple_Clicked;
                return Globals.Factory.GetRibbonFactory().CreateRibbonManager(new Microsoft.Office.Tools.Ribbon.IRibbonExtension[] { r });
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        private void RibbonButtonMultiple_Clicked(double? value)
        {
            if (value != null)
                DivideAndMultiply(value.Value);
        }

        private void RibbonButtonDivide_Clicked(double? value)
        {
            if (value != null)
                DivideAndMultiply(1 / value.Value);
        }

        private void DivideAndMultiply(double value)
        {
            try
            {
                Excel.Application excelApp = Globals.ThisAddIn.Application as Microsoft.Office.Interop.Excel.Application;
                Excel.Range selectionRange = excelApp.Selection as Excel.Range;
                if (selectionRange == null)
                {
                    System.Windows.Forms.MessageBox.Show("Выдзеленая вобласць не з'яўляецца дыяпазонам!", "Будзьце больш уважлівымі", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                    return;
                }

                if (selectionRange.Value.GetType().Name == "Double")
                {
                    double d = 0;
                    if (Double.TryParse(selectionRange.Value.ToString(), out d))
                    {
                        selectionRange.Value = d * value;
                    }
                }
                else
                {
                    object[,] array = selectionRange.Value;
                    int rank = array.Rank;
                    int lbound = array.GetLowerBound(0);
                    int ubound = array.GetUpperBound(0);

                    int count = selectionRange.Count;
                    if (count > 1)
                    {
                        array = selectionRange.Value;
                        for (int colInd = 1; colInd <= selectionRange.Columns.Count; colInd++)
                            for (int i = lbound; i <= ubound; i++)
                            {
                                double d = 0;
                                if (Double.TryParse(array[i, colInd].ToString(), out d))
                                {
                                    array[i, colInd] = d * value;
                                }
                            }
                        selectionRange.Value = array;
                    }
                }
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(String.Format("Узнікла памылка!\n{0}\n{1}", e.Message, e.StackTrace),
                    "Памылка", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
            }
        }
    }
}
