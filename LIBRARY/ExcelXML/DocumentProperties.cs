using System;
using System.Xml;
using TMP.Extensions;

namespace TMP.ExcelXml
{
	/// <summary>
	/// Gets or sets document properties
	/// </summary>
	public class DocumentProperties
	{
		#region Public Properties
		/// <summary>
		/// Gets or sets the author of the workbook
		/// </summary>
		public string Author { get; set; }
		/// <summary>
		/// Gets or sets the last author of the workbook
		/// </summary>
		public string LastAuthor { get; set; }
		/// <summary>
		/// Gets or sets the manager of the workbook
		/// </summary>
		public string Manager { get; set; }
		/// <summary>
		/// Gets or sets the company of the workbook
		/// </summary>
		public string Company { get; set; }
		/// <summary>
		/// Gets or sets the subject of the workbook
		/// </summary>
		public string Subject { get; set; }
		/// <summary>
		/// Gets or sets the title of the workbook
		/// </summary>
		public string Title { get; set; }
		/// <summary>
		/// Gets or sets the date and time creation workbook
		/// </summary>
		public DateTime? Created { get; set; }
		#endregion

		#region Constructor
		/// <summary>
		/// Creates an instance with empty document properties
		/// </summary>
		public DocumentProperties()
		{
			Author = "";
			LastAuthor = "";
			Manager = "";
			Company = "";
			Subject = "";
			Title = "";
			Created = DateTime.UtcNow;
		}
		#endregion

		#region Export
		internal void Export(XmlWriter writer)
		{
			// DocumentProperties
			writer.WriteStartElement("", "DocumentProperties", "urn:schemas-microsoft-com:office:office");

			if (!Author.IsNullOrEmpty())
				writer.WriteElementString("Author", Author);
			if (!LastAuthor.IsNullOrEmpty())
				writer.WriteElementString("LastAuthor", LastAuthor);
			if (!Manager.IsNullOrEmpty())
				writer.WriteElementString("Manager", Manager);
			if (!Company.IsNullOrEmpty())
				writer.WriteElementString("Company", Company);
			if (!Subject.IsNullOrEmpty())
				writer.WriteElementString("Subject", Subject);
			if (!Title.IsNullOrEmpty())
				writer.WriteElementString("Title", Title);
			if (Created.HasValue)
			{
				DateTime date = Created.Value;
				date = date.ToUniversalTime();
				writer.WriteElementString("Created", date.ToString("yyyy-MM-ddTHH:mm:ssZ"));
			}
			else
				writer.WriteElementString("Created", String.Empty);
			writer.WriteEndElement();
		}
		#endregion

		#region Import
		internal void Import(XmlReader reader)
		{
			while (reader.Read() && !(reader.Name == "DocumentProperties" && reader.NodeType == XmlNodeType.EndElement))
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					switch (reader.Name)
					{
						// Document Properties
						case "Author":
							{
								if (reader.IsEmptyElement)
									continue;

								reader.Read();
								if (reader.NodeType == XmlNodeType.Text)
									Author = reader.Value;

								break;
							}
						case "LastAuthor":
							{
								if (reader.IsEmptyElement)
									continue;

								reader.Read();
								if (reader.NodeType == XmlNodeType.Text)
									LastAuthor = reader.Value;

								break;
							}
						case "Manager":
							{
								if (reader.IsEmptyElement)
									continue;

								reader.Read();
								if (reader.NodeType == XmlNodeType.Text)
									Manager = reader.Value;

								break;
							}
						case "Company":
							{
								if (reader.IsEmptyElement)
									continue;

								reader.Read();
								if (reader.NodeType == XmlNodeType.Text)
									Company = reader.Value;

								break;
							}
						case "Subject":
							{
								if (reader.IsEmptyElement)
									continue;

								reader.Read();
								if (reader.NodeType == XmlNodeType.Text)
									Subject = reader.Value;

								break;
							}
						case "Title":
							{
								if (reader.IsEmptyElement)
									continue;

								reader.Read();
								if (reader.NodeType == XmlNodeType.Text)
									Title = reader.Value;

								break;
							}
						case "Created":
							{
								if (reader.IsEmptyElement)
									continue;

								reader.Read();
								if (reader.NodeType == XmlNodeType.Text)
								{
									DateTime date = DateTime.MinValue;
									bool flag = DateTime.TryParse(reader.Value, out date);
									if (flag)
										Created = date;
									else
										Created = null;
								}
								break;
							}
					}
				}
			}
		}
		#endregion
	}
}
