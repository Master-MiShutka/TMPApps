using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;

namespace Xceed.Wpf.DataGrid.Export
{
    internal static class FormatHelper
    {
        internal static void CsvFormatCell(object cellValue, bool firstCell, bool lastCell, StringBuilder sb, IFormatSettings formatSettings)
        {
            if (cellValue != null)
            {
                string outputString = string.Empty;
                bool checkWhitespace = true;

                if ((!Convert.IsDBNull(cellValue)) && (!(cellValue is Array)))
                {
                    Type dataType = cellValue.GetType();

                    if (dataType == typeof(string))
                    {
                        string textQualifier = formatSettings.TextQualifier.ToString();

                        if (textQualifier == "\0")
                        {
                            outputString = (string)cellValue;
                        }
                        else
                        {
                            outputString = formatSettings.TextQualifier + ((string)cellValue).Replace(textQualifier, textQualifier + textQualifier) + formatSettings.TextQualifier;
                        }

                        checkWhitespace = false;
                    }
                    else if (dataType == typeof(DateTime))
                    {
                        if (!string.IsNullOrEmpty(formatSettings.DateTimeFormat))
                        {
                            if (formatSettings.Culture == null)
                            {
                                outputString = ((DateTime)cellValue).ToString(formatSettings.DateTimeFormat, CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                outputString = ((DateTime)cellValue).ToString(formatSettings.DateTimeFormat, formatSettings.Culture);
                            }
                        }
                    }
                    else if ((dataType == typeof(double)) ||
                             (dataType == typeof(decimal)) ||
                             (dataType == typeof(float)) ||
                             (dataType == typeof(int)) ||
                             (dataType == typeof(double)) ||
                             (dataType == typeof(decimal)) ||
                             (dataType == typeof(float)) ||
                             (dataType == typeof(short)) ||
                             (dataType == typeof(Single)) ||
                             (dataType == typeof(UInt16)) ||
                             (dataType == typeof(UInt32)) ||
                             (dataType == typeof(UInt64)) ||
                             (dataType == typeof(Int16)) ||
                             (dataType == typeof(Int64)))
                    {
                        string format = formatSettings.NumericFormat;

                        if (((dataType == typeof(double)) ||
                              (dataType == typeof(decimal)) ||
                              (dataType == typeof(float))) &&
                            (!string.IsNullOrEmpty(formatSettings.FloatingPointFormat)))
                            format = formatSettings.FloatingPointFormat;

                        if (!string.IsNullOrEmpty(format))
                        {
                            if (formatSettings.Culture == null)
                            {
                                outputString = string.Format(CultureInfo.InvariantCulture, "{0:" + format + "}", cellValue);
                            }
                            else
                            {
                                outputString = string.Format(formatSettings.Culture, "{0:" + format + "}", cellValue);
                            }
                        }
                    }
                }
                // For dates and numbers, as a rule, we never use the TextQualifier. However, the
                // specified format can introduce whitespaces. To better support this case, we add
                // the TextQualifier if needed.
                if ((checkWhitespace) && (formatSettings.TextQualifier != '\0'))
                {
                    bool useTextQualifier = false;

                    // If the output string contains the character used to separate the fields, we
                    // don't bother checking for whitespace. TextQualifier will be used.
                    if (outputString.IndexOf(formatSettings.Separator) < 0)
                    {
                        for (int i = 0; i < outputString.Length; i++)
                        {
                            if (char.IsWhiteSpace(outputString, i))
                            {
                                useTextQualifier = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        useTextQualifier = true;
                    }

                    if (useTextQualifier)
                        outputString = formatSettings.TextQualifier + outputString + formatSettings.TextQualifier;
                }

                sb.Append(outputString);

                if (!lastCell)
                    sb.Append(formatSettings.Separator);
                else
                    sb.Append(formatSettings.NewLine);
            }
        }
        internal static void HtmlFormatCell(object cellValue, bool firstCell, bool lastCell, StringBuilder sb)
        {
            string style = string.Empty;
            Type dataType = cellValue.GetType();
            if (dataType == typeof(string))
            {
                style = " class=text";
            }

            if (firstCell)
            {
                // First cell - append start of row
                sb.Append("<TR>");
            }

            sb.AppendFormat("<TD{0}>", style); // Start cell
            if (cellValue != null)
            {
                FormatPlainTextAsHtml(cellValue.ToString(), new StringWriter(sb, CultureInfo.CurrentCulture));
            }
            else
            {
                sb.Append("&nbsp;");
            }

            sb.Append("</TD>"); // End cell
            if (lastCell)
            {
                // Last cell - append end of row
                sb.Append("</TR>");
            }
        }


        internal static void GetClipboardContentForHtml(StringBuilder content)
        {
            const string HTML_PREFIX = "Version:1.0\r\nStartHTML:00000097\r\nEndHTML:{0}\r\nStartFragment:00000133\r\nEndFragment:{1}\r\n";
            const string HTML_STYLES_AND_START_FRAGMENT =
    @"<HTML>
<HEAD>
<META http-equiv=Content-Type content=""text/html; charset=utf-8"">
<STYLE>
<!--
td
	{padding-top:1px;
	padding-right:1px;
	padding-left:1px;
	mso-ignore:padding;
	mso-number-format:General;
	white-space:nowrap;
    text-align:general;
	vertical-align:middle;
    border:.5pt solid windowtext;}
.text
    {mso-number-format:""@"";}
-->
</STYLE>
</HEAD>
<BODY>
<!--StartFragment-->";
            const string HTML_END_FRAGMENT = "\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>";

            content.Insert(0, "<TABLE cellpadding=0 cellspacing=0>");
            content.Append("</TABLE>");
            // DDVSO: 104825 - The character set supported by the clipboard is Unicode in its UTF-8 encoding.
            // There are characters in Asian languages which require more than 2 bytes for encoding into UTF-8
            // Marshal.SystemDefaultCharSize is 2 and would not be appropriate in all cases. We have to explicitly calculate the number of bytes.  
            byte[] sourceBytes = Encoding.Unicode.GetBytes(content.ToString());
            byte[] destinationBytes = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, sourceBytes);

            // Byte count of context before the content in the HTML format
            int bytecountPrefixContext = string.Format(CultureInfo.InvariantCulture, HTML_PREFIX, "00000000", "00000000").Length +
                HTML_STYLES_AND_START_FRAGMENT.Length;
            // Byte count of context after the content in the HTML format
            int bytecountSuffixContext = HTML_END_FRAGMENT.Length;

            int bytecountEndOfFragment = bytecountPrefixContext + destinationBytes.Length;
            int bytecountEndOfHtml = bytecountEndOfFragment + bytecountSuffixContext;
            string prefix = string.Format(CultureInfo.InvariantCulture,
                HTML_PREFIX,
                bytecountEndOfHtml.ToString("00000000", CultureInfo.InvariantCulture),
                bytecountEndOfFragment.ToString("00000000", CultureInfo.InvariantCulture)) + HTML_STYLES_AND_START_FRAGMENT;
            content.Insert(0, prefix);
            content.Append(HTML_END_FRAGMENT);
        }

        // Code taken from ASP.NET file xsp\System\Web\httpserverutility.cs; same in DataGridViewCell.cs
        private static void FormatPlainTextAsHtml(string s, TextWriter output)
        {
            if (s == null)
            {
                return;
            }

            int cb = s.Length;
            char prevCh = '\0';

            for (int i = 0; i < cb; i++)
            {
                char ch = s[i];
                switch (ch)
                {
                    case '<':
                        output.Write("&lt;");
                        break;
                    case '>':
                        output.Write("&gt;");
                        break;
                    case '"':
                        output.Write("&quot;");
                        break;
                    case '&':
                        output.Write("&amp;");
                        break;
                    case ' ':
                        if (prevCh == ' ')
                        {
                            output.Write("&nbsp;");
                        }
                        else
                        {
                            output.Write(ch);
                        }

                        break;
                    case '\r':
                        // Ignore \r, only handle \n
                        break;
                    case '\n':
                        output.Write("<br>");
                        break;

                    // 
                    default:
                        // The seemingly arbitrary 160 comes from RFC
                        if (ch >= 160 && ch < 256)
                        {
                            output.Write("&#");
                            output.Write(((int)ch).ToString(NumberFormatInfo.InvariantInfo));
                            output.Write(';');
                        }
                        else
                        {
                            output.Write(ch);
                        }

                        break;
                }

                prevCh = ch;
            }
        }
    }
}
