namespace TMP.ExcelXml
{
    using System;
    using System.Xml;
    using TMP.Extensions;

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
            this.Author = string.Empty;
            this.LastAuthor = string.Empty;
            this.Manager = string.Empty;
            this.Company = string.Empty;
            this.Subject = string.Empty;
            this.Title = string.Empty;
            this.Created = DateTime.UtcNow;
        }
        #endregion

        #region Export
        internal void Export(XmlWriter writer)
        {
            // DocumentProperties
            writer.WriteStartElement(string.Empty, "DocumentProperties", "urn:schemas-microsoft-com:office:office");

            if (!this.Author.IsNullOrEmpty())
            {
                writer.WriteElementString("Author", this.Author);
            }

            if (!this.LastAuthor.IsNullOrEmpty())
            {
                writer.WriteElementString("LastAuthor", this.LastAuthor);
            }

            if (!this.Manager.IsNullOrEmpty())
            {
                writer.WriteElementString("Manager", this.Manager);
            }

            if (!this.Company.IsNullOrEmpty())
            {
                writer.WriteElementString("Company", this.Company);
            }

            if (!this.Subject.IsNullOrEmpty())
            {
                writer.WriteElementString("Subject", this.Subject);
            }

            if (!this.Title.IsNullOrEmpty())
            {
                writer.WriteElementString("Title", this.Title);
            }

            if (this.Created.HasValue)
            {
                DateTime date = this.Created.Value;
                date = date.ToUniversalTime();
                writer.WriteElementString("Created", date.ToString("yyyy-MM-ddTHH:mm:ssZ"));
            }
            else
            {
                writer.WriteElementString("Created", string.Empty);
            }

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
                                {
                                    continue;
                                }

                                reader.Read();
                                if (reader.NodeType == XmlNodeType.Text)
                                {
                                    this.Author = reader.Value;
                                }

                                break;
                            }

                        case "LastAuthor":
                            {
                                if (reader.IsEmptyElement)
                                {
                                    continue;
                                }

                                reader.Read();
                                if (reader.NodeType == XmlNodeType.Text)
                                {
                                    this.LastAuthor = reader.Value;
                                }

                                break;
                            }

                        case "Manager":
                            {
                                if (reader.IsEmptyElement)
                                {
                                    continue;
                                }

                                reader.Read();
                                if (reader.NodeType == XmlNodeType.Text)
                                {
                                    this.Manager = reader.Value;
                                }

                                break;
                            }

                        case "Company":
                            {
                                if (reader.IsEmptyElement)
                                {
                                    continue;
                                }

                                reader.Read();
                                if (reader.NodeType == XmlNodeType.Text)
                                {
                                    this.Company = reader.Value;
                                }

                                break;
                            }

                        case "Subject":
                            {
                                if (reader.IsEmptyElement)
                                {
                                    continue;
                                }

                                reader.Read();
                                if (reader.NodeType == XmlNodeType.Text)
                                {
                                    this.Subject = reader.Value;
                                }

                                break;
                            }

                        case "Title":
                            {
                                if (reader.IsEmptyElement)
                                {
                                    continue;
                                }

                                reader.Read();
                                if (reader.NodeType == XmlNodeType.Text)
                                {
                                    this.Title = reader.Value;
                                }

                                break;
                            }

                        case "Created":
                            {
                                if (reader.IsEmptyElement)
                                {
                                    continue;
                                }

                                reader.Read();
                                if (reader.NodeType == XmlNodeType.Text)
                                {
                                    DateTime date = DateTime.MinValue;
                                    bool flag = DateTime.TryParse(reader.Value, out date);
                                    if (flag)
                                    {
                                        this.Created = date;
                                    }
                                    else
                                    {
                                        this.Created = null;
                                    }
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
