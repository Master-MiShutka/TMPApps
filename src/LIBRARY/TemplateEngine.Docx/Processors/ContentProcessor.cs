﻿namespace TemplateEngine.Docx.Processors
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;

    internal class ContentProcessor
    {
        private bool isNeedToRemoveContentControls;

        private readonly List<IProcessor> processors;

        internal ContentProcessor(ProcessContext context)
        {
            this.processors = new List<IProcessor>
            {
                new FieldsProcessor(),
                new RepeatProcessor(context),
                new TableProcessor(context),
                new ListProcessor(context),
                new ImagesProcessor(context),
            };
        }

        public ContentProcessor SetRemoveContentControls(bool isNeedToRemove)
        {
            this.isNeedToRemoveContentControls = isNeedToRemove;
            foreach (var processor in this.processors)
            {
                processor.SetRemoveContentControls(this.isNeedToRemoveContentControls);
            }

            return this;
        }

        public ProcessResult FillContent(XElement content, IEnumerable<IContentItem> data)
        {
            var result = ProcessResult.NotHandledResult;
            var processedItems = new List<IContentItem>();
            data = data.ToList();

            foreach (var contentItems in data.GroupBy(d => d.Name))
            {
                if (processedItems.Any(i => i.Name == contentItems.Key))
                {
                    continue;
                }

                var contentControls = this.FindContentControls(content, contentItems.Key).ToList();

                // Need to get error message from processor.
                if (!contentControls.Any())
                {
                    contentControls.Add(null);
                }

                foreach (var xElement in contentControls)
                {
                    if (contentItems.Any(item => item is TableContent) && xElement != null)
                    {
                        var processTableFieldsResult = this.ProcessTableFields(data.OfType<FieldContent>(), xElement);
                        processedItems.AddRange(processTableFieldsResult.HandledItems);

                        result.Merge(processTableFieldsResult);
                    }

                    foreach (var processor in this.processors)
                    {
                        var processorResult = processor.FillContent(xElement, contentItems);

                        processedItems.AddRange(processorResult.HandledItems);
                        result.Merge(processorResult);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Processes table data that should not be duplicated
        /// </summary>
        /// <param name="fields">Possible fields</param>
        /// <param name="xElement">Table content control</param>
        /// <returns>List of content items that were processed</returns>
        private ProcessResult ProcessTableFields(IEnumerable<FieldContent> fields, XElement xElement)
        {
            var processResult = ProcessResult.NotHandledResult;
            foreach (var fieldContentControl in fields)
            {
                var innerContentControls = this.FindContentControls(xElement.Element(W.sdtContent), fieldContentControl.Name);
                foreach (var innerContentControl in innerContentControls)
                {
                    var processor = this.processors.OfType<FieldsProcessor>().FirstOrDefault();
                    if (processor != null)
                    {
                        var result = processor.FillContent(innerContentControl, fieldContentControl);
                        processResult.Merge(result);
                    }
                }
            }

            return processResult;
        }

        public ProcessResult FillContent(XElement content, Content data)
        {
            return this.FillContent(content, data.AsEnumerable());
        }

        public ProcessResult FillContent(XElement content, IContentItem data)
        {
            return this.FillContent(content, new List<IContentItem> { data });
        }

        private IEnumerable<XElement> FindContentControls(XElement content, string tagName)
        {
            return content

                // top level content controls
                .FirstLevelDescendantsAndSelf(W.sdt)

                // with specified tagName
                .Where(sdt => tagName == sdt.SdtTagName());
        }
    }
}
