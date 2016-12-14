using System;
using TMP.Extensions;

namespace TMP.ExcelXml
{
	internal class NamedRange
	{
		#region Private and Internal fields
		internal Range Range;
		internal Worksheet Worksheet;
		internal string Name;
		#endregion

		#region Constructor
		internal NamedRange(Range range, string name, Worksheet ws)
		{
			if (range == null)
				throw new ArgumentNullException("range");

			if (name.IsNullOrEmpty())
				throw new ArgumentNullException("name");

			Worksheet = ws;
			Range = range;
			Name = name;
		}
		#endregion
	}
}
