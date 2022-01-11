namespace TMP.UI.Controls.WPF.NativeShellDialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class Filter
    {
        private static readonly char[] extensionTrimStart = new char[] { ' ', '.', ';' };

        private static readonly char[] extensionTrim = new char[] { ' ', '.', ';', '*', '\\', '/', '?' };

        public Filter(string displayName, params string[] extensions)
            : this(displayName, (IEnumerable<string>)extensions)
        {
        }

        public Filter(string displayName, IEnumerable<string> extensions)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new ArgumentNullException(nameof(displayName));
            }

            if (extensions == null)
            {
                throw new ArgumentNullException(nameof(extensions));
            }

            this.DisplayName = displayName.Trim();
            this.Extensions = extensions
                .Select(s => s.Trim()) // Trim whitespace
                .Select(s => // Trim leading
                   s.StartsWith("*.", StringComparison.OrdinalIgnoreCase) ? s.Substring(2) :
                   s.StartsWith(".", StringComparison.OrdinalIgnoreCase) ? s.Substring(1) : s)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList(); // make a copy to prevent possible changes
        }

        public string DisplayName { get; }

        public IReadOnlyList<string> Extensions { get; }

        /// <summary>Returns null if the string couldn't be parsed.</summary>
        public static Filter[] ParseWindowsFormsFilter(string filter)
        {
            // https://msdn.microsoft.com/en-us/library/system.windows.forms.filedialog.filter(v=vs.110).aspx
            if (string.IsNullOrWhiteSpace(filter))
            {
                return null;
            }

            string[] components = filter.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            if (components.Length % 2 != 0)
            {
                return null;
            }

            Filter[] filters = new Filter[components.Length / 2];
            int fi = 0;
            for (int i = 0; i < components.Length; i += 2)
            {
                string displayName = components[i];
                string extensionsCat = components[i + 1];

                string[] extensions = extensionsCat.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                filters[fi] = new Filter(displayName, extensions);
                fi++;
            }

            return filters;
        }

        internal string ToFilterSpecString()
        {
            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach (string extension in this.Extensions)
            {
                if (!first)
                {
                    sb.Append(';');
                }

                first = false;

                sb.Append("*.");
                sb.Append(extension);
            }

            return sb.ToString();
        }

        internal void ToExtensionList(StringBuilder sb)
        {
            bool first = true;
            foreach (string extension in this.Extensions)
            {
                if (!first)
                {
                    sb.Append(", ");
                }

                first = false;

                sb.Append("*.");
                sb.Append(extension);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(this.DisplayName);
            sb.Append(" (");
            this.ToExtensionList(sb);
            sb.Append(")");
            return sb.ToString();
        }

        internal FilterSpec ToFilterSpec()
        {
            string filter = this.ToFilterSpecString();
            return new FilterSpec(this.DisplayName, filter);
        }
    }
}
