using System;
using System.Collections.Generic;

namespace TMP.Work.Emcos
{
    public static class AnswerParser
    {
        public static ICollection<Controls.VTreeView.ITreeNode> ArchivesPoint(string data)
        {
            var list = Utils.ParseRecords(data);

            if (list == null)
                return null;

            ICollection<Controls.VTreeView.ITreeNode> result = new List<Controls.VTreeView.ITreeNode>();

            foreach (var nvc in list)
            {
                var gr = new Model.GRTreeNode();
                for (int i = 0; i < nvc.Count; i++)
                {
                    #region Разбор полей
                    switch (nvc.GetKey(i))
                    {
                        case "GR_ID":
                            gr.Id = nvc[i];
                            break;
                        case "POINT_ID":
                            gr.Id = nvc[i];
                            break;
                        case "GR_CODE":
                            gr.Code = nvc[i];
                            break;
                        case "POINT_CODE":
                            gr.Code = nvc[i];
                            break;
                        case "GR_NAME":
                            gr.Name = nvc[i];
                            break;
                        case "POINT_NAME":
                            gr.Name = nvc[i];
                            break;
                        case "GR_TYPE_ID":
                            int intValue = 0;
                            Int32.TryParse(nvc[i], out intValue);
                            gr.Type_Id = intValue;
                            break;
                        case "GR_TYPE_NAME":
                            gr.Type_Name = nvc[i];
                            break;
                        case "POINT_TYPE_NAME":
                            gr.Type_Name = nvc[i];
                            break;
                        case "GR_TYPE_CODE":
                            gr.Type_Code = nvc[i];
                            break;
                        case "POINT_TYPE_CODE":
                            gr.Type_Code = nvc[i];
                            break;
                        case "PARENT":
                            gr.Parent = nvc[i];
                            break;
                        case "HASCHILDS":
                            byte byteValue = 0;
                            Byte.TryParse(nvc[i], out byteValue);
                            gr.HasChilds = byteValue;
                            break;
                        case "TYPE":
                            gr.Type = nvc[i];
                            break;
                        case "ECP_NAME":
                            gr.Ecp_Name = nvc[i];
                            break;
                        default:
                            break;
                    }
                    #endregion                    
                }
                result.Add(gr);
            }
            return result;
        }
    }
}
