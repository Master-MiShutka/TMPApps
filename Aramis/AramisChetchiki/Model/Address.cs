namespace TMP.WORK.AramisChetchiki.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Runtime.Serialization;
    using System.Text.RegularExpressions;

    public interface IAddress : IComparable, IComparable<IAddress>, IEquatable<IAddress>
    {
        string ТипНаселённогоПункта { get; }

        string НаселённыйПункт { get; }

        string Улица { get; }

        string Дом { get; }

        string Квартира { get; }

        string УлицаСДомом { get; }

        string УлицаСДомомИКв { get; }
    }

    [MessagePack.MessagePackObject]
    public class BaseAddress : IAddress, IComparable<BaseAddress>, IEquatable<BaseAddress>
    {
        public BaseAddress()
        {
        }

        public BaseAddress(string cityType, string city, string street, string houseNumber, string apartment)
        {
            this.ТипНаселённогоПункта = cityType;
            this.НаселённыйПункт = city;
            this.Улица = string.IsNullOrWhiteSpace(street) ? string.Empty : street;
            this.Дом = string.IsNullOrWhiteSpace(houseNumber) ? string.Empty : houseNumber;
            this.Квартира = string.IsNullOrWhiteSpace(apartment) ? string.Empty : apartment;
        }

        public BaseAddress(string city, string street, string houseNumber, string apartment)
        {
            if (string.IsNullOrWhiteSpace(city) == false)
            {
                string[] parts = city.Split('.');

                if (parts.Length == 2)
                {
                    this.ТипНаселённогоПункта = parts[0] + ".";
                    this.НаселённыйПункт = parts[1];
                }
                else if (parts.Length == 1)
                {
                    this.ТипНаселённогоПункта = parts[0].StartsWith("Дач")
                        ? "дачи"
                        : "?";
                    this.НаселённыйПункт = parts[0];
                }
                else
                {
                    this.ТипНаселённогоПункта = string.Empty;
                    this.НаселённыйПункт = city;
                }
            }
            else
            {
                this.НаселённыйПункт = this.ТипНаселённогоПункта = string.Empty;
            }

            this.Улица = string.IsNullOrWhiteSpace(street) ? string.Empty : street;
            this.Дом = string.IsNullOrWhiteSpace(houseNumber) ? string.Empty : houseNumber;
            this.Квартира = string.IsNullOrWhiteSpace(apartment) ? string.Empty : apartment;
        }

        [SummaryInfo]
        [DisplayName("Тип населённого пункта")]
        [Display(GroupName = "Адрес")]
        [MessagePack.Key(0)]
        public string ТипНаселённогоПункта { get; }

        [SummaryInfo]
        [DisplayName("Населённый пункт")]
        [Display(GroupName = "Адрес")]
        [MessagePack.Key(1)]
        public string НаселённыйПункт { get; }

        [DisplayName("Улица")]
        [Display(GroupName = "Адрес")]
        [MessagePack.Key(2)]
        public string Улица { get; }

        [DisplayName("Дом")]
        [Display(GroupName = "Адрес")]
        [MessagePack.Key(3)]
        public string Дом { get; }

        [DisplayName("Квартира")]
        [Display(GroupName = "Адрес")]
        [MessagePack.Key(4)]
        public string Квартира { get; }

        [IgnoreDataMember]
        [DisplayName("Улица с домом")]
        [Display(GroupName = "Адрес")]
        public string УлицаСДомом => string.IsNullOrWhiteSpace(this.Улица) ? (string.IsNullOrWhiteSpace(this.Дом) ? string.Empty : this.Дом) : this.Улица + ", " + this.Дом;

        [IgnoreDataMember]
        [DisplayName("Улица с домом и квартирой")]
        [Display(GroupName = "Адрес")]
        public string УлицаСДомомИКв => (string.IsNullOrWhiteSpace(this.Улица) && string.IsNullOrWhiteSpace(this.Дом))
            ? (string.IsNullOrEmpty(this.Квартира) ? string.Empty : $", {this.Квартира}")
            : this.УлицаСДомом + (string.IsNullOrEmpty(this.Квартира) ? string.Empty : $", {this.Квартира}");

        public override string ToString()
        {
            string kv = string.IsNullOrWhiteSpace(this.Квартира) ? string.Empty : $", {this.Квартира}";

            return $"{this.ТипНаселённогоПункта}{this.НаселённыйПункт}, {this.УлицаСДомом}{kv}";
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.ТипНаселённогоПункта, this.НаселённыйПункт, this.УлицаСДомом, this.Квартира);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            BaseAddress other = obj as BaseAddress;
            return this.Equals(other);
        }

        public int CompareTo(IAddress other)
        {
            if (other == null)
            {
                return 1;
            }

            int compareStrOrInt(string value1, string value2)
            {
                Regex regex = new Regex("^(\\d+)");

                // run the regex on both strings
                Match xRegexResult = regex.Match(value1);
                Match yRegexResult = regex.Match(value2);

                // check if they are both numbers
                if (xRegexResult.Success && yRegexResult.Success)
                {
                    int xx = int.Parse(xRegexResult.Groups[1].Value, AppSettings.CurrentCulture.NumberFormat);
                    int yy = int.Parse(yRegexResult.Groups[1].Value, AppSettings.CurrentCulture.NumberFormat);
                    return xx.CompareTo(yy);
                }

                // otherwise return as string comparison
                return string.Compare(value1, value2, AppSettings.StringComparisonMethod);
            }

            int compareHouses(string house1, string house2, string kv1, string kv2)
            {
                int result = compareStrOrInt(house1, house2);
                return result == 0 ? compareStrOrInt(kv1, kv2) : result;
            }

            if (string.Equals(this.НаселённыйПункт, other.НаселённыйПункт, AppSettings.StringComparisonMethod))
            {
                if (string.IsNullOrWhiteSpace(this.Улица))
                {
                    return compareHouses(this.Дом, other.Дом, this.Квартира, other.Квартира);
                }
                else
                {
                    if (string.Equals(this.Улица, other.Улица, AppSettings.StringComparisonMethod))
                    {
                        return compareHouses(this.Дом, other.Дом, this.Квартира, other.Квартира);
                    }
                    else
                    {
                        return string.Compare(this.Улица, other.Улица, AppSettings.StringComparisonMethod);
                    }
                }
            }
            else
            {
                return string.Compare(this.НаселённыйПункт, other.НаселённыйПункт, AppSettings.StringComparisonMethod);
            }
        }

        public bool Equals(IAddress other)
        {
            return other != null
                && string.Equals(this.ТипНаселённогоПункта, other.ТипНаселённогоПункта, AppSettings.StringComparisonMethod)
                && string.Equals(this.НаселённыйПункт, other.НаселённыйПункт, AppSettings.StringComparisonMethod)
                && string.Equals(this.УлицаСДомом, other.УлицаСДомом, AppSettings.StringComparisonMethod)
                && string.Equals(this.Квартира, other.Квартира, AppSettings.StringComparisonMethod);
        }

        public int CompareTo(BaseAddress other)
        {
            return this.CompareTo(other as IAddress);
        }

        public bool Equals(BaseAddress other)
        {
            return this.Equals(other as IAddress);
        }

        public int CompareTo(object obj)
        {
            return this.CompareTo(obj as IAddress);
        }

        public static bool operator ==(BaseAddress left, BaseAddress right)
        {
            return left is null ? right is null : left.Equals(right);
        }

        public static bool operator !=(BaseAddress left, BaseAddress right)
        {
            return !(left == right);
        }

        public static bool operator <(BaseAddress left, BaseAddress right)
        {
            return left is null ? right is not null : left.CompareTo(right) < 0;
        }

        public static bool operator <=(BaseAddress left, BaseAddress right)
        {
            return left is null || left.CompareTo(right) <= 0;
        }

        public static bool operator >(BaseAddress left, BaseAddress right)
        {
            return left is not null && left.CompareTo(right) > 0;
        }

        public static bool operator >=(BaseAddress left, BaseAddress right)
        {
            return left is null ? right is null : left.CompareTo(right) >= 0;
        }
    }

    [MessagePack.MessagePackObject]
    public sealed class Address : BaseAddress, IComparable, IComparable<Address>, IEquatable<Address>
    {
        public Address()
        {
        }

        public Address(string cityType, string city, string street, string houseNumber, string apartment, string province)
            : base(cityType, city, street, houseNumber, apartment)
        {
            this.СельскийСовет = string.IsNullOrWhiteSpace(province) ? string.Empty : province;

            string s = this.CityAndStreetWithHouse;
            if (DictionaryStreetWithHouseNumber.ContainsKey(s) == false)
            {
                DictionaryStreetWithHouseNumber.AddOrUpdate(s, 1u, (string key, uint value) => { return value; });
            }
            else
            {
                DictionaryStreetWithHouseNumber[s] = DictionaryStreetWithHouseNumber[s] + 1;
            }
        }

        public Address(string city, string street, string houseNumber, string apartment, string province)
            : base(city, street, houseNumber, apartment)
        {
            this.СельскийСовет = string.IsNullOrWhiteSpace(province) ? string.Empty : province;

            string s = this.CityAndStreetWithHouse;
            if (DictionaryStreetWithHouseNumber.ContainsKey(s) == false)
            {
                DictionaryStreetWithHouseNumber.AddOrUpdate(s, 1u, (string key, uint value) => { return value; });
            }
            else
            {
                DictionaryStreetWithHouseNumber[s] = DictionaryStreetWithHouseNumber[s] + 1;
            }
        }

        [SummaryInfo]
        [DisplayName("Сельский совет")]
        [Display(GroupName = "Адрес")]
        [MessagePack.Key(5)]
        public string СельскийСовет { get; }

        [MessagePack.Key(6)]
        public static HashSet<string> Cities { get; } = new HashSet<string>();

        [MessagePack.Key(7)]
        public static System.Collections.Concurrent.ConcurrentDictionary<string, uint> DictionaryStreetWithHouseNumber { get; } = new System.Collections.Concurrent.ConcurrentDictionary<string, uint>();

        [MessagePack.IgnoreMember]
        [SummaryInfo]
        [DisplayName("МЖД")]
        [Display(GroupName = "Адрес")]
        public bool ЭтоМжд => DictionaryStreetWithHouseNumber.ContainsKey(this.CityAndStreetWithHouse)
            && DictionaryStreetWithHouseNumber[this.CityAndStreetWithHouse] >= AppSettings.Default.NumberOfApartmentsInAnApartmentBuilding;

        [MessagePack.IgnoreMember]
        public string CityAndStreetWithHouse => this.НаселённыйПункт + ", " + this.УлицаСДомом;

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            Address other = obj as Address;
            return this.Equals(other);
        }

        public int CompareTo(Address other)
        {
            return this.CompareTo(other as IAddress);
        }

        public bool Equals(Address other)
        {
            return this.Equals(other as IAddress);
        }

        public static bool operator ==(Address left, Address right)
        {
            return left is null ? right is null : left.Equals(right);
        }

        public static bool operator !=(Address left, Address right)
        {
            return !(left == right);
        }

        public static bool operator <(Address left, Address right)
        {
            return left is null ? right is not null : left.CompareTo(right) < 0;
        }

        public static bool operator <=(Address left, Address right)
        {
            return left is null || left.CompareTo(right) <= 0;
        }

        public static bool operator >(Address left, Address right)
        {
            return left is not null && left.CompareTo(right) > 0;
        }

        public static bool operator >=(Address left, Address right)
        {
            return left is null ? right is null : left.CompareTo(right) >= 0;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.ТипНаселённогоПункта, this.НаселённыйПункт, this.УлицаСДомом, this.Квартира, this.СельскийСовет);
        }
    }

    public class StringLikeNumbersComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            Regex regex = new Regex("^(d+)");

            // run the regex on both strings
            Match xRegexResult = regex.Match(x);
            Match yRegexResult = regex.Match(y);

            // check if they are both numbers
            if (xRegexResult.Success && yRegexResult.Success)
            {
                int xx = int.Parse(xRegexResult.Groups[1].Value, AppSettings.CurrentCulture.NumberFormat);
                int yy = int.Parse(yRegexResult.Groups[1].Value, AppSettings.CurrentCulture.NumberFormat);
                return xx.CompareTo(yy);
            }

            // otherwise return as string comparison
            return string.Compare(x, y, AppSettings.StringComparisonMethod);
        }
    }
}
