namespace TMP.WORK.AramisChetchiki.Model
{
    using System;

    [MessagePack.MessagePackObject]
    public class TransformerSubstation : IComparable<TransformerSubstation>, IEquatable<TransformerSubstation>
    {
        [MessagePack.IgnoreMember]
        private StringComparison stringComparison = StringComparison.Ordinal;

        public TransformerSubstation(string type, int number, string name)
        {
            this.Type = type;
            this.Number = number;
            this.Name = name;
        }

        [MessagePack.Key(0)]
        public string Type { get; set; }

        [MessagePack.Key(1)]
        public int Number { get; set; }

        [MessagePack.Key(2)]
        public string Name { get; set; }

        [MessagePack.Key(3)]
        public bool IsEmpty => string.IsNullOrWhiteSpace(this.Type) & (this.Number == 0 || this.Number == -1);

        public override string ToString()
        {
            string n = this.Number < 0 ? string.Empty : this.Number.ToString(System.Globalization.CultureInfo.InvariantCulture);
            return $"{this.Type} {n} {this.Name}".Trim();
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Number, this.Type, this.Name);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            TransformerSubstation other = obj as TransformerSubstation;
            if (other != null)
            {
                return this.Number == other.Number && string.Equals(this.Type, other.Type, this.stringComparison);
            }
            else
            {
                return false;
            }
        }

        public bool Equals(TransformerSubstation other)
        {
            if (other == null)
            {
                return false;
            }

            return this.Number == other.Number && string.Equals(this.Type, other.Type, this.stringComparison);
        }

        public int CompareTo(TransformerSubstation other)
        {
            if (other == null)
            {
                return 1;
            }

            if (other != null)
            {
                return this.Number.CompareTo(other.Number);
            }
            else
            {
                throw new ArgumentException("Object is not a TransformerSubstation");
            }
        }
    }
}
