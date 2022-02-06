namespace TMP.WORK.AramisChetchiki.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Text.RegularExpressions;

    public interface IAddress : IComparable, IComparable<IAddress>, IEquatable<IAddress>
    {
        string CityType { get; }

        string City { get; }

        string Street { get; }

        string HouseNumber { get; }

        string Apartment { get; }

        string StreetWithHouseNumber { get; }

        string StreetWithHouseNumberAndApartment { get; }
    }

    [MessagePack.MessagePackObject(keyAsPropertyName: true)]
    public sealed class Address :IAddress, IComparable, IComparable<Address>, IEquatable<Address>
    {
        [MessagePack.IgnoreMember]
        public static System.Collections.Concurrent.ConcurrentDictionary<string, uint> DictionaryStreetWithHouseNumber { get; private set; } = new System.Collections.Concurrent.ConcurrentDictionary<string, uint>();

        [MessagePack.SerializationConstructor]
        public Address(string city, string street, string houseNumber, string apartment, string province, string cityType = null)
        {
            if (string.IsNullOrEmpty(cityType))
            {
                if (string.IsNullOrWhiteSpace(city) == false)
                {
                    string[] parts = city.Split('.');

                    if (parts.Length == 2)
                    {
                        this.CityType = parts[0] + ".";
                        this.City = parts[1];
                    }
                    else if (parts.Length == 1)
                    {
                        this.CityType = parts[0].StartsWith("Дач")
                            ? "дачи"
                            : "?";
                        this.City = parts[0];
                    }
                    else
                    {
                        this.CityType = string.Empty;
                        this.City = city;
                    }
                }
                else
                {
                    this.City = this.CityType = string.Empty;
                }
            }
            else
            {
                this.City = city;
                this.CityType = cityType;
            }

            this.Street = string.IsNullOrWhiteSpace(street) ? string.Empty : street;
            this.HouseNumber = string.IsNullOrWhiteSpace(houseNumber) ? string.Empty : houseNumber;
            this.Apartment = string.IsNullOrWhiteSpace(apartment) ? string.Empty : apartment;

            this.Province = string.IsNullOrWhiteSpace(province) ? string.Empty : province;

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

        [DisplayName("Населённый пункт")]
        [Display(GroupName = "Адрес")]
        [MessagePack.Key(nameof(City))]
        public string City { get; }

        [DisplayName("Улица")]
        [Display(GroupName = "Адрес")]
        [MessagePack.Key(nameof(Street))]
        public string Street { get; }

        [DisplayName("Дом")]
        [Display(GroupName = "Адрес")]
        [MessagePack.Key(nameof(HouseNumber))]
        public string HouseNumber { get; }

        [DisplayName("Квартира")]
        [Display(GroupName = "Адрес")]
        [MessagePack.Key(nameof(Apartment))]
        public string Apartment { get; }

        [DisplayName("Сельский совет")]
        [Display(GroupName = "Адрес")]
        [MessagePack.Key(nameof(Province))]
        public string Province { get; }

        [DisplayName("Тип населённого пункта")]
        [Display(GroupName = "Адрес")]
        [MessagePack.Key(nameof(CityType))]
        public string CityType { get; }

        [MessagePack.IgnoreMember]
        [DisplayName("Улица с домом")]
        [Display(GroupName = "Адрес")]
        public string StreetWithHouseNumber => string.IsNullOrWhiteSpace(this.Street) ? (string.IsNullOrWhiteSpace(this.HouseNumber) ? string.Empty : this.HouseNumber) : this.Street + ", " + this.HouseNumber;

        [MessagePack.IgnoreMember]
        [DisplayName("Улица с домом и квартирой")]
        [Display(GroupName = "Адрес")]
        public string StreetWithHouseNumberAndApartment => (string.IsNullOrWhiteSpace(this.Street) && string.IsNullOrWhiteSpace(this.HouseNumber))
            ? (string.IsNullOrEmpty(this.Apartment) ? string.Empty : $", {this.Apartment}")
            : this.StreetWithHouseNumber + (string.IsNullOrEmpty(this.Apartment) ? string.Empty : $", {this.Apartment}");

        [MessagePack.IgnoreMember]
        [DisplayName("МЖД")]
        [Display(GroupName = "Адрес")]
        public bool IsApartmentBuilding => DictionaryStreetWithHouseNumber.ContainsKey(this.CityAndStreetWithHouse)
            && DictionaryStreetWithHouseNumber[this.CityAndStreetWithHouse] >= AppSettings.Default.NumberOfApartmentsInAnApartmentBuilding;

        [MessagePack.IgnoreMember]
        public string CityAndStreetWithHouse => this.City + ", " + this.StreetWithHouseNumber;

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            Address other = obj as Address;
            return this.Equals(other);
        }

        public override string ToString()
        {
            string kv = string.IsNullOrWhiteSpace(this.Apartment) ? string.Empty : $", {this.Apartment}";

            return $"{this.CityType}{this.City}, {this.StreetWithHouseNumber}{kv}";
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.CityType, this.City, this.StreetWithHouseNumber, this.Apartment, this.Province);
        }

        public bool Equals(IAddress other)
        {
            return other != null
                && string.Equals(this.CityType, other.CityType, AppSettings.StringComparisonMethod)
                && string.Equals(this.City, other.City, AppSettings.StringComparisonMethod)
                && string.Equals(this.StreetWithHouseNumber, other.StreetWithHouseNumber, AppSettings.StringComparisonMethod)
                && string.Equals(this.Apartment, other.Apartment, AppSettings.StringComparisonMethod);
        }

        public bool Equals(Address other)
        {
            return this.Equals(other as IAddress);
        }

        #region IComparable implementation
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

            if (string.Equals(this.City, other.City, AppSettings.StringComparisonMethod))
            {
                if (string.IsNullOrWhiteSpace(this.Street))
                {
                    return compareHouses(this.HouseNumber, other.HouseNumber, this.Apartment, other.Apartment);
                }
                else
                {
                    if (string.Equals(this.Street, other.Street, AppSettings.StringComparisonMethod))
                    {
                        return compareHouses(this.HouseNumber, other.HouseNumber, this.Apartment, other.Apartment);
                    }
                    else
                    {
                        return string.Compare(this.Street, other.Street, AppSettings.StringComparisonMethod);
                    }
                }
            }
            else
            {
                return string.Compare(this.City, other.City, AppSettings.StringComparisonMethod);
            }
        }

        public int CompareTo(Address other)
        {
            return this.CompareTo(other as IAddress);
        }

        public int CompareTo(object obj)
        {
            return this.CompareTo(obj as IAddress);
        }

        #endregion

        #region Operators

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

        #endregion
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
