namespace TemplateEngine.Docx
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;

    internal class NumberingAccessor
    {

        private readonly XDocument numberingPart;

        private readonly Dictionary<int, int> lastNumIds;

        private static readonly Random Random = new Random();

        internal NumberingAccessor(XDocument numberingPart, Dictionary<int, int> lastNumIds)
        {
            this.numberingPart = numberingPart;

            this.lastNumIds = lastNumIds;
        }

        public void ResetNumbering(IEnumerable<XElement> elements)
        {
            var numPrs = elements.Descendants(W.numPr).Where(d => d.Element(W.numId) != null);

            foreach (var numPr in numPrs.GroupBy(e => (int)e.Element(W.numId).Attribute(W.val)))
            {
                var numId = numPr.Key;
                var numIds = numPrs.Elements(W.numId).Attributes(W.val).Where(e => (int)e == numId);
                var ilvl = int.Parse(numPr.First().Element(W.ilvl).Attribute(W.val).Value);

                if (this.lastNumIds.ContainsKey(ilvl))
                {

                    var numElementPrototype = this.numberingPart
                    .Descendants(W.num)
                    .FirstOrDefault(n => (int)n.Attribute(W.numId) == numId);

                    var abstractNumElementPrototype = this.numberingPart
                        .Descendants(W.abstractNum)
                        .FirstOrDefault(e => e.Attribute(W.abstractNumId).Value ==
                                numElementPrototype
                                .Element(W.abstractNumId)
                                .Attribute(W.val).Value);
                    var lastNumElement = this.numberingPart
                        .Descendants(W.num)
                        .OrderBy(n => (int)n.Attribute(W.numId))
                        .LastOrDefault();
                    if (lastNumElement == null)
                    {
                        break;
                    }

                    var nextNumId = (int)lastNumElement.Attribute(W.numId) + 1;

                    var lastAbstractNumElement = this.numberingPart.Descendants(W.abstractNum).Last();
                    var lastAbstractNumId = (int)lastAbstractNumElement.Attribute(W.abstractNumId);

                    var newAbstractNumElement = new XElement(abstractNumElementPrototype);
                    newAbstractNumElement.Attribute(W.abstractNumId).SetValue(lastAbstractNumId + 1);

                    var next = Random.Next(int.MaxValue);
                    var nsid = newAbstractNumElement.Element(W.nsid);
                    if (nsid != null)
                    {
                        nsid.Attribute(W.val).SetValue(next.ToString("X"));
                    }

                    lastAbstractNumElement.AddAfterSelf(newAbstractNumElement);

                    var newNumElement = new XElement(numElementPrototype);
                    newNumElement.Attribute(W.numId).SetValue(nextNumId);
                    newNumElement.Element(W.abstractNumId).Attribute(W.val).SetValue(lastAbstractNumId + 1);
                    lastNumElement.AddAfterSelf(newNumElement);

                    foreach (var xElement in numIds)
                    {
                        xElement.SetValue(nextNumId);
                    }

                    this.lastNumIds[ilvl] = nextNumId;
                }
                else
                {
                    this.lastNumIds.Add(ilvl, numId);
                }
            }
        }
    }
}
