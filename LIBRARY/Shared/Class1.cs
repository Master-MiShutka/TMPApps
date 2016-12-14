using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARMTES.Shared
{
    public class Class1
    {
    }
}

/*

public void Save(System.Windows.Data.CollectionView items)
{
    XDocument xdoc = new XDocument();

    XElement xeRoot = new XElement("Data");
    XElement xeSubRoot = new XElement("Rows");

    foreach (var item in items)
    {
        ListViewData lvc = (ListViewData)item;

        XElement xRow = new XElement("Row");
        xRow.Add(new XElement("col1", lvc.Col1));
        xRow.Add(new XElement("col2", lvc.Col2));

        xeSubRoot.Add(xRow);
    }
    xeRoot.Add(xeSubRoot);
    xdoc.Add(xeRoot);

    xdoc.Save("MyData.xml");
}

    // Create the query 
        var rowsFromFile = from c in XDocument.Load(
                            "MyData.xml").Elements(
                            "Data").Elements("Rows").Elements("Row")
                                   select c;

*/
